using System.Linq;
using Godot;
using YKnyttLib;

public class GDKnyttCamera : Camera2D
{
    public GDKnyttGame Game { get; private set; }

    public void initialize(GDKnyttGame game)
    {
        this.Game = game;
    }

    // Give position in global space
    public void jumpTo(Vector2 position)
    {
        GlobalPosition = position;
    }

    private static float smoothDamp(float current, float target, ref float current_velocity, float smooth_time, float delta)
    {
        smooth_time = Mathf.Max(0.0001f, smooth_time);
        float omega = 2f / smooth_time;
        float x = omega * delta;
        float exp = 1f / (1f + x + 0.48f * x * x + 0.235f * x * x * x);
        float delta_value = current - target;
        float temp = (current_velocity + omega * delta_value) * delta;
        current_velocity = (current_velocity - omega * temp) * exp;
        float result = target + (delta_value + temp) * exp;
        if ((target - current > 0f) == (result > target))
        {
            result = target;
            current_velocity = 0f;
        }
        return result;
    }

    private const float LEAD_FRACTION = 0.8f;
    private const float CAMERA_TRANSITION_TIME = 0.35f;
    private const float OFFSET_TRANSITION_TIME = 0.60f;
    private const float BACKWARD_LIMIT = 10f;
    
    private float currentOffset = 0f;
    private float offsetVelocity = 0f;
    private float cameraXVelocity = 0f;
    private float cameraInArea = 0f;
    private bool leftAreaRestricted, rightAreaRestricted;
    private bool hasLeftRestriction, hasRightRestriction;

    public enum Place { SameArea, Slide, Reset, Preserve }

    public void adjustScroll(float delta, Place new_area_camera)
    {
        if (GetViewport() == null) { return; }

        float camera_global_position_x = GlobalPosition.x;
        float x_viewport = GetViewport().GetVisibleRect().Size.x * TouchSettings.ViewportNow;
        float look_ahead = x_viewport * (LEAD_FRACTION - 0.5f);
        float target_offset = Game.Juni.FacingRight ? look_ahead : -look_ahead;
                
        if (new_area_camera != Place.SameArea)
        {
            Game.GDWorld.Areas.Areas.TryGetValue(Game.CurrentArea.Area.Position + new KnyttPoint(-1, 0), out var left_area);
            leftAreaRestricted = !GDKnyttSettings.SeamlessScroll || left_area == null || left_area.Area.Empty ||
                (Game.CurrentArea.Area.Warp.LoadedWarp && !Game.CurrentArea.Area.Warp.WarpLeft.isZero()) ||
                left_area.Area.FlagWarps.Any(w => w != null);

            Game.GDWorld.Areas.Areas.TryGetValue(Game.CurrentArea.Area.Position + new KnyttPoint(1, 0), out var right_area);
            rightAreaRestricted = !GDKnyttSettings.SeamlessScroll || right_area == null || right_area.Area.Empty ||
                (Game.CurrentArea.Area.Warp.LoadedWarp && !Game.CurrentArea.Area.Warp.WarpRight.isZero()) ||
                right_area.Area.FlagWarps.Any(w => w != null);

            if (new_area_camera == Place.Slide)
            {
                if (hasLeftRestriction  && !leftAreaRestricted)  { new_area_camera = Place.Reset; }
                if (hasRightRestriction && !rightAreaRestricted) { new_area_camera = Place.Reset; }
            }
            
            if (new_area_camera == Place.Preserve)
            {
                camera_global_position_x = Game.CurrentArea.GlobalCenter.x + cameraInArea;
            }
        }

        float juni_on_screen = (Game.Juni.GlobalPosition.x - GlobalPosition.x) / x_viewport + 0.5f;
        float ahead_fraction = Game.Juni.FacingRight ? 1f - juni_on_screen : juni_on_screen;
        float backward_movement = Mathf.Abs(GlobalPosition.x - currentOffset - Game.Juni.GlobalPosition.x);
        bool stop_camera = backward_movement < BACKWARD_LIMIT && ahead_fraction > LEAD_FRACTION && !Game.Juni.DebugFlyMode;

        if (new_area_camera == Place.Reset)
        {
            currentOffset = target_offset;
            camera_global_position_x = currentOffset + Game.Juni.GlobalPosition.x;
            stop_camera = true;
        }

        currentOffset = smoothDamp(currentOffset, target_offset, ref offsetVelocity, OFFSET_TRANSITION_TIME, delta);
        camera_global_position_x = smoothDamp(camera_global_position_x, Game.Juni.GlobalPosition.x + currentOffset, 
                                              ref cameraXVelocity, CAMERA_TRANSITION_TIME, delta);
                                    
        float camera_restriction = (600f - x_viewport) / 2f;
        cameraInArea = camera_global_position_x - Game.CurrentArea.GlobalCenter.x;

        hasRightRestriction = rightAreaRestricted && cameraInArea >= camera_restriction;
        if (hasRightRestriction)
        {
            cameraInArea = camera_restriction;
            if (currentOffset < target_offset) { stop_camera = true; } // if camera is moving to the right but right area is restricted
        }
        hasLeftRestriction = leftAreaRestricted && cameraInArea <= -camera_restriction;
        if (hasLeftRestriction)
        {
            cameraInArea = -camera_restriction;
            if (currentOffset > target_offset) { stop_camera = true; }
        }
        if (leftAreaRestricted && rightAreaRestricted && camera_restriction < 0)
        {
            cameraInArea = 0;
        }

        GlobalPosition = new Vector2(Game.CurrentArea.GlobalCenter.x + cameraInArea, Game.CurrentArea.GlobalCenter.y);
        
        if (stop_camera)
        {
            currentOffset = GlobalPosition.x - Game.Juni.GlobalPosition.x;
            offsetVelocity = cameraXVelocity = 0f;
        }

        if (x_viewport <= 600)
        {
            float juni_on_camera = Game.Juni.GlobalPosition.x - Game.Camera.GlobalPosition.x;
            foreach (var area in Game.GDWorld.Areas.Areas.Values)
            {
                float juni_in_this_area = Game.Juni.GlobalPosition.x - area.GlobalCenter.x;
                area.Background.Position = new Vector2((juni_in_this_area - juni_on_camera) * 0.5f, 0);
            }
        }
    }
}
