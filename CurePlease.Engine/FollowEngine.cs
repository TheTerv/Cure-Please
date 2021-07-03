using CurePlease.Model;
using CurePlease.Model.Config;
using EliteMMO.API;
using System;
using System.Timers;
using static EliteMMO.API.EliteAPI;

namespace CurePlease.Engine
{
    public class FollowEngine
    {
        public EliteAPI _PowerLeveler { get; set; }

        public EliteAPI _Monitored { get; set; }

        public MySettings _Config { get; set; }

        private static int? _FollowerId { get; set; }

        private bool _Running { get; set; }

        private int _StuckCount { get; set; }

        private bool _StuckWarning { get; set; }

        private int _ToFarToFollowWarning { get; set; }

        private Coordinates _LastPLCoordinates;

        private Timer _FollowEngineTimer = new Timer();

        public FollowEngine() 
        {
            _FollowEngineTimer.Elapsed += new ElapsedEventHandler(Follow_DoWork);
            _FollowEngineTimer.Interval = 1000;
            _FollowEngineTimer.Enabled = true;
        }

        public void Setup(EliteAPI pl, EliteAPI monitored, MySettings config)
        {
            _PowerLeveler = pl;
            _Monitored = monitored;
            _Config = config;
        }        

        public static void ClearFollowing()
        {
            _FollowerId = null;
        }

        public void Stop()
        {
            _Running = false;

            // This id will change when zoning, etc
            _FollowerId = null;

            _LastPLCoordinates = null;

            Reset();
        }

        public void Start()
        {
            _Running = true;

            _LastPLCoordinates = new Coordinates(_PowerLeveler.Player.X, _PowerLeveler.Player.Y, _PowerLeveler.Player.Z);
        }

        public bool IsMoving()
        {
            if (_PowerLeveler == null || _LastPLCoordinates == null)
                return false;

            return _PowerLeveler.AutoFollow.IsAutoFollowing 
                // in case we're using /follow
                || 0.1 < _LastPLCoordinates.GetDistanceFrom(_PowerLeveler.Player.X, _PowerLeveler.Player.Y, _PowerLeveler.Player.Z);
        }

        private void Reset()
        {
            if (_Config != null && !_Config.FFXIDefaultAutoFollow)
            {
                _PowerLeveler.AutoFollow.IsAutoFollowing = false;
            }

            _StuckWarning = false;
            _StuckCount = 0;
        }

        private void FollowingUsingFFXICommand(int whoToFollowId)
        {
            // IF THE CURRENT TARGET IS NOT THE FOLLOWERS TARGET ID THEN CHANGE THAT NOW
            if (_PowerLeveler.Target.GetTargetInfo().TargetIndex != whoToFollowId)
            {
                // FIRST REMOVE THE CURRENT TARGET
                _PowerLeveler.Target.SetTarget(0);

                // NOW SET THE NEXT TARGET AFTER A WAIT
                System.Threading.Thread.Sleep(TimeSpan.FromSeconds(0.1));

                _PowerLeveler.Target.SetTarget(whoToFollowId);
            }
            // IF THE TARGET IS CORRECT BUT YOU'RE NOT LOCKED ON THEN DO SO NOW
            else if (_PowerLeveler.Target.GetTargetInfo().TargetIndex == whoToFollowId && !_PowerLeveler.Target.GetTargetInfo().LockedOn)
            {
                _PowerLeveler.ThirdParty.SendString("/lockon <t>");
            }
            // EVERYTHING SHOULD BE FINE SO FOLLOW THEM
            else
            {
                _PowerLeveler.ThirdParty.SendString("/follow");
            }
        }

        private bool ShouldUpdateFollow(XiEntity whoToFollow)
        {
            // if we're moving
            if (_PowerLeveler.AutoFollow.IsAutoFollowing)
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

        public void Follow_DoWork(object source, ElapsedEventArgs e)
        {
            _FollowEngineTimer.Stop();

            if (_PowerLeveler == null || _Config == null || !_Running)
            {
                _FollowEngineTimer.Start();
                return;
            }

            // We'll use this to detect if we're moving
            _LastPLCoordinates.UpdateCoordinates(_PowerLeveler.Player.X, _PowerLeveler.Player.Y, _PowerLeveler.Player.Z);

            // MAKE SURE BOTH ELITEAPI INSTANCES ARE ACTIVE, THE BOT ISN'T PAUSED, AND THERE IS AN AUTOFOLLOWTARGET NAMED
            if (!string.IsNullOrEmpty(_Config.autoFollowName))
            {
                // RUN THE FUNCTION TO GRAB THE ID OF THE FOLLOW TARGET THIS ALSO MAKES SURE THEY ARE IN RANGE TO FOLLOW
                int whoToFollowId = GetFollowIDForPlayer(_Config.autoFollowName.ToLower());

                // If the FOLLOWER'S ID is NOT -1 THEN THEY WERE LOCATED SO CONTINUE THE CHECKS
                if (whoToFollowId != -1)
                {
                    // GRAB THE FOLLOW TARGETS ENTITY TABLE TO CHECK DISTANCE ETC
                    XiEntity followTarget = _PowerLeveler.Entity.GetEntity(whoToFollowId);

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

            _FollowEngineTimer.Start();
        }

        private int GetFollowIDForPlayer(string playerName)
        {
            if (!string.IsNullOrEmpty(playerName))
            {
                if (_FollowerId != null)
                {
                    return _FollowerId.Value;
                }

                for (int x = 0; x < 2048; x++)
                {
                    XiEntity entity = _PowerLeveler.Entity.GetEntity(x);

                    if (entity.Name != null && entity.Name.ToLower().Equals(playerName))
                    {
                        _FollowerId = Convert.ToInt32(entity.TargetID);
                        return _FollowerId.Value;
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

            while (Math.Truncate(followTarget.Distance) >= (int)_Config.autoFollowDistance)
            {
                // It appears this can be toggled to false when getting stuck, so we need it within the loop
                _PowerLeveler.AutoFollow.IsAutoFollowing = true;

                float Player_X = _PowerLeveler.Player.X;
                float Player_Y = _PowerLeveler.Player.Y;
                float Player_Z = _PowerLeveler.Player.Z;

                Target_X = followTarget.X;
                Target_Y = followTarget.Y;
                Target_Z = followTarget.Z;

                float dX = Target_X - Player_X;
                float dY = Target_Y - Player_Y;
                float dZ = Target_Z - Player_Z;

                _PowerLeveler.AutoFollow.SetAutoFollowCoords(dX, dY, dZ);

                _LastPLCoordinates.UpdateCoordinates(_PowerLeveler.Player.X, _PowerLeveler.Player.Y, _PowerLeveler.Player.Z);

                System.Threading.Thread.Sleep(TimeSpan.FromSeconds(0.25));

                // STUCK CHECKER
                double distance = _LastPLCoordinates.GetDistanceFrom(_PowerLeveler.Player.X, _PowerLeveler.Player.Y, _PowerLeveler.Player.Z);

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
            if (_Monitored != null && _Monitored.Player.Name != _PowerLeveler.Player.Name)
            {
                string createdTell = "/tell " + _Monitored.Player.Name + " " + Message;
                _PowerLeveler.ThirdParty.SendString(createdTell);
                _ToFarToFollowWarning = 1;
            }
        }
    }
}
