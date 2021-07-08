using CurePlease.Model;
using CurePlease.Model.Config;
using EliteMMO.API;
using Microsoft.Extensions.Logging;
using System;
using System.Timers;
using static EliteMMO.API.EliteAPI;

namespace CurePlease.Engine
{
    public class FollowEngine : IFollowEngine
    {
        private readonly ILogger<FollowEngine> _Logger;

        private EliteAPI _PowerLeveler;

        private MySettings _Config;

        private static int? _FollowerId;

        private bool _Running;

        private int _StuckCount;

        private bool _StuckWarning;

        private int _ToFarToFollowWarning;

        private Coordinates _LastPLCoordinates;

        private readonly Timer _FollowEngineTimer = new();

        public FollowEngine(ILogger<FollowEngine> logger) 
        {
            _Logger = logger;

            _FollowEngineTimer.Elapsed += new ElapsedEventHandler(Follow_DoWork);
            _FollowEngineTimer.Interval = 1000;
            _FollowEngineTimer.Enabled = true;
        }

        public void Setup(EliteAPI pl, MySettings config)
        {
            _PowerLeveler = pl;
            _Config = config;

            Start();
        }

        public void Start()
        {
            _Running = true;

            try
            {
                _LastPLCoordinates = new Coordinates(_PowerLeveler.Player.X, _PowerLeveler.Player.Y, _PowerLeveler.Player.Z);
            }
            catch (Exception ex)
            {
                _Logger.LogError("Exception occurred when starting FollowEngine", ex);
            }
        }

        public void Stop()
        {
            _Running = false;

            // This id will change when zoning, etc
            _FollowerId = null;

            _LastPLCoordinates = null;

            Reset();
        }

        public bool IsMoving()
        {
            if (_PowerLeveler == null || _LastPLCoordinates == null)
                return false;

            try
            {
                return _PowerLeveler.AutoFollow.IsAutoFollowing
                    // in case we're using /follow
                    || 0.1 < _LastPLCoordinates.GetDistanceFrom(_PowerLeveler.Player.X, _PowerLeveler.Player.Y, _PowerLeveler.Player.Z);
            }
            catch(Exception ex)
            {
                _Logger.LogError("Exception occurred when checking if character moving", ex);
                return false;
            }
        }

        // temporary solution until we decompose UI from logic
        public static void ClearFollowing()
        {
            _FollowerId = null;
        }

        public void Follow_DoWork(object source, ElapsedEventArgs e)
        {
            _FollowEngineTimer.Stop();

            try
            {
                if (_PowerLeveler == null || _PowerLeveler.Player == null || _LastPLCoordinates == null|| _Config == null || !_Running)
                {
                    _FollowEngineTimer.Start();
                    return;
                }
            
                // We'll use this to detect if we're moving
                // NOTE: This throws an occassional NULL exception but when debugging nothing is null.. wrapping in try/catch to try and recover
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
            }
            catch (Exception ex)
            {
                _Logger.LogError("Unexpected error occurred while running following engine", ex);
            }

            _FollowEngineTimer.Start();
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

                    if (_Config.autoFollow_Warning == true && _StuckWarning != true && _StuckCount == 10)
                    {
                        //IssueMessage(_Monitored.Player.Name, "I appear to be stuck.");
                        _StuckWarning = true;
                    }
                }
            }
        }

        private void IssueMessage(string whoToTell, string message)
        {
            if (!string.IsNullOrWhiteSpace(whoToTell) && !string.IsNullOrWhiteSpace(message) && whoToTell != _PowerLeveler.Player.Name)
            {
                string createdTell = "/tell " + whoToTell + " " + message;
                _PowerLeveler.ThirdParty.SendString(createdTell);
                _ToFarToFollowWarning = 1;
            }
        }

        private void Reset()
        {
            if (_Config != null && !_Config.FFXIDefaultAutoFollow)
            {
                _PowerLeveler.AutoFollow.IsAutoFollowing = false;
            }

            _FollowerId = null;
            _StuckWarning = false;
            _StuckCount = 0;
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
                    //IssueMessage("You're too far to follow.");
                    _ToFarToFollowWarning = 1;
                }
            }

            return tooFar;
        }
    }
}
