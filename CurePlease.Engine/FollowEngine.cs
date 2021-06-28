using CurePlease.Model;
using CurePlease.Model.Config;
using EliteMMO.API;
using System;
using System.Threading;
using static EliteMMO.API.EliteAPI;

namespace CurePlease.Engine
{
    public class FollowEngine
    {
        public EliteAPI _PL { get; set; }

        public EliteAPI _Monitored { get; set; }

        public MySettings _Config { get; set; }

        public FollowEngine() { }

        public void Setup(EliteAPI pl, EliteAPI monitored, MySettings config)
        {
            _PL = pl;
            _Monitored = monitored;
            _Config = config;
        }

        public static void ClearFollowing()
        {
            FollowerId = null;
        }
        
        private static int? FollowerId { get; set; }

        private bool _Running { get; set; }

        public void Stop()
        {
            _Running = false;

            Reset();
        }

        public void Start()
        {
            _Running = true;
        }

        private int _StuckCount { get; set; }
        private bool _StuckWarning { get; set; }
        private int _ToFarToFollowWarning { get; set; }

        private Coordinates _LastPLCoordinates = new Coordinates(0, 0, 0);

        private void Reset()
        {
            if (_Config != null && !_Config.FFXIDefaultAutoFollow)
            {
                _PL.AutoFollow.IsAutoFollowing = false;
            }

            _StuckWarning = false;
            _StuckCount = 0;
        }

        private void FollowingUsingFFXICommand(int whoToFollowId)
        {
            // IF THE CURRENT TARGET IS NOT THE FOLLOWERS TARGET ID THEN CHANGE THAT NOW
            if (_PL.Target.GetTargetInfo().TargetIndex != whoToFollowId)
            {
                // FIRST REMOVE THE CURRENT TARGET
                _PL.Target.SetTarget(0);

                // NOW SET THE NEXT TARGET AFTER A WAIT
                Thread.Sleep(TimeSpan.FromSeconds(0.1));

                _PL.Target.SetTarget(whoToFollowId);
            }
            // IF THE TARGET IS CORRECT BUT YOU'RE NOT LOCKED ON THEN DO SO NOW
            else if (_PL.Target.GetTargetInfo().TargetIndex == whoToFollowId && !_PL.Target.GetTargetInfo().LockedOn)
            {
                _PL.ThirdParty.SendString("/lockon <t>");
            }
            // EVERYTHING SHOULD BE FINE SO FOLLOW THEM
            else
            {
                _PL.ThirdParty.SendString("/follow");
            }
        }

        private bool ShouldUpdateFollow(XiEntity whoToFollow)
        {
            // if we're moving
            if (_PL.AutoFollow.IsAutoFollowing)
                return false;

            double currentTargetDistance = Math.Truncate(whoToFollow.Distance);

            if (currentTargetDistance < (int)_Config.autoFollowDistance)
                return false;

            if (TooFarToFollow(currentTargetDistance))
                return false;

            return true;
        }

        private bool TooFarToFollow(double currentTargetDistance)
        {
            var tooFar = currentTargetDistance > 40;

            // YOU ARE NOT AT NOR FURTHER THAN THE DISTANCE REQUIRED SO CANCEL ELITEAPI AUTOFOLLOW
            if (tooFar)
            {
                // IF YOU ARE TOO FAR TO FOLLOW THEN STOP AND IF ENABLED WARN THE MONITORED PLAYER
                if (_Config.autoFollow_Warning == true && _ToFarToFollowWarning == 0)
                {
                    IssueMessage("You're too far to follow.");
                    _ToFarToFollowWarning = 1;
                }
            }

            return tooFar;
        }

        public void Follow_BGW_DoWork(object sender, System.ComponentModel.DoWorkEventArgs e)
        {
            if (_PL == null || _Config == null || !_Running)
            {
                Thread.Sleep(TimeSpan.FromSeconds(1));
                return;
            }

            // MAKE SURE BOTH ELITEAPI INSTANCES ARE ACTIVE, THE BOT ISN'T PAUSED, AND THERE IS AN AUTOFOLLOWTARGET NAMED
            if (!string.IsNullOrEmpty(_Config.autoFollowName))
            {
                // RUN THE FUNCTION TO GRAB THE ID OF THE FOLLOW TARGET THIS ALSO MAKES SURE THEY ARE IN RANGE TO FOLLOW
                int whoToFollowId = GetFollowIDForPlayer(_Config.autoFollowName.ToLower());

                // If the FOLLOWER'S ID is NOT -1 THEN THEY WERE LOCATED SO CONTINUE THE CHECKS
                if (whoToFollowId != -1)
                {
                    // GRAB THE FOLLOW TARGETS ENTITY TABLE TO CHECK DISTANCE ETC
                    XiEntity followTarget = _PL.Entity.GetEntity(whoToFollowId);

                    // We're being being able to follow, nothing to do
                    if (ShouldUpdateFollow(followTarget))
                    {
                        _ToFarToFollowWarning = 0;

                        // SQUARE ENIX FINAL FANTASY XI DEFAULT AUTO FOLLOW
                        if (_Config.FFXIDefaultAutoFollow)
                        {
                            FollowingUsingFFXICommand(whoToFollowId);
                        }
                        // ELITEAPI'S IMPROVED AUTO FOLLOW
                        else
                        {
                            MoveToTarget(followTarget);
                            Reset();
                        }
                    }
                }
            }

            Thread.Sleep(TimeSpan.FromSeconds(1));
        }

        private int GetFollowIDForPlayer(string playerName)
        {
            if (!string.IsNullOrEmpty(playerName))
            {
                if (FollowerId != null)
                {
                    return FollowerId.Value;
                }

                for (int x = 0; x < 2048; x++)
                {
                    XiEntity entity = _PL.Entity.GetEntity(x);

                    if (entity.Name != null && entity.Name.ToLower().Equals(playerName))
                    {
                        FollowerId = Convert.ToInt32(entity.TargetID);
                        return FollowerId.Value;
                    }
                }
            }

            return -1;
        }

        private void MoveToTarget(XiEntity followTarget)
        {
            float Target_X;
            float Target_Y;
            float Target_Z;

            _PL.AutoFollow.IsAutoFollowing = true;

            while (Math.Truncate(followTarget.Distance) >= (int)_Config.autoFollowDistance)
            {
                float Player_X = _PL.Player.X;
                float Player_Y = _PL.Player.Y;
                float Player_Z = _PL.Player.Z;

                Target_X = followTarget.X;
                Target_Y = followTarget.Y;
                Target_Z = followTarget.Z;

                float dX = Target_X - Player_X;
                float dY = Target_Y - Player_Y;
                float dZ = Target_Z - Player_Z;

                _PL.AutoFollow.SetAutoFollowCoords(dX, dY, dZ);

                _LastPLCoordinates.UpdateCoordinates(_PL.Player.X, _PL.Player.Y, _PL.Player.Z);

                Thread.Sleep(TimeSpan.FromSeconds(0.25));

                // STUCK CHECKER
                double distance = _LastPLCoordinates.GetDistanceFrom(_PL.Player.X, _PL.Player.Y, _PL.Player.Z);

                if (distance < .1)
                {
                    _StuckCount++;

                    if (_Config.autoFollow_Warning == true && _StuckWarning != true && followTarget.Name == _Monitored.Player.Name && _StuckCount == 10)
                    {
                        IssueMessage("I appear to be stuck.");
                        _StuckWarning = true;
                    }
                }
            }
        }

        private void IssueMessage(string Message)
        {
            if (_Monitored != null && _Monitored.Player.Name != _PL.Player.Name)
            {
                string createdTell = "/tell " + _Monitored.Player.Name + " " + Message;
                _PL.ThirdParty.SendString(createdTell);
                _ToFarToFollowWarning = 1;
            }
        }
    }
}
