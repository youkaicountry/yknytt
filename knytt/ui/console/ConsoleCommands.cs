using Godot;
using YKnyttLib.Parser;
using YKnyttLib;
using System;

public static class ConsoleCommands
{
    public static CommandSet BuildCommandSet()
    {
        var cs = new CommandSet();
        cs.AddCommand(new CommandDeclaration("speed", "View or set the speed of the game", null, false, SpeedCommand.NewSpeedCommand, new CommandArg("value", CommandArg.Type.FloatArg, optional: true)));
        cs.AddCommand(new CommandDeclaration("help", "View help for a given command", null, false, HelpCommand.NewHelpCommand, new CommandArg("name", CommandArg.Type.StringArg, optional: true)));
        cs.AddCommand(new CommandDeclaration("list", "View the list of commands", null, false, ListCommand.NewListCommand));
        cs.AddCommand(new CommandDeclaration("save", "Saves current game. Also copies and pastes save file from clipboard.", 
            "save: saves game at current position\n" +
            "save print: prints save file\n" +
            "save copy: copies save to clipboard\n" +
            "save paste: pastes save from clipboard", 
            false, SaveCommand.NewSaveCommand, new CommandArg("subcmd", CommandArg.Type.StringArg, optional: true)));
        cs.AddCommand(new CommandDeclaration("set", "Sets Juni's power or flag",
            "powers: run climb doublejump highjump eye enemydetector umbrella hologram " + 
            "redkey yellowkey bluekey purplekey map flag0 .. flag9\nvalue: on off 1 0",
            false, SetCommand.NewSetCommand, new CommandArg("variable", CommandArg.Type.StringArg, optional: false), 
            new CommandArg("value", CommandArg.Type.StringArg, optional: false)));
        cs.AddCommand(new CommandDeclaration("monitor", "Monitors current area position. Also can monitor Juni's flags.", 
            "monitor: turn on monitor, display always on top\n" +
            "monitor flash: display only when value changes (like '-' on keyboard)\n" +
            "monitor flags: display also Juni's flags\n" +
            "monitor off: turn off monitor", 
            false, MonitorCommand.NewMonitorCommand, new CommandArg("subcmd", CommandArg.Type.StringArg, optional: true)));
        cs.AddCommand(new CommandDeclaration("idclip", "Gives ability to go through walls. idclip off: normal mode", null, false, FlyCommand.NewFlyCommand, new CommandArg("on", CommandArg.Type.StringArg, optional: true)));
        cs.AddCommand(new CommandDeclaration("iddqd", "Gives invulnerability. iddqd off: normal mode", null, false, ImmuneCommand.NewImmuneCommand, new CommandArg("on", CommandArg.Type.StringArg, optional: true)));
        cs.AddCommand(new CommandDeclaration("map", "Enables KS+ map for bare KS levels", null, false, MapCommand.NewMapCommand, new CommandArg("on", CommandArg.Type.StringArg, optional: true)));
        cs.AddCommand(new CommandDeclaration("shift", "Shifts Juni (relative to current position)", null, false, ShiftCommand.NewShiftCommand, 
            new CommandArg("xMap", CommandArg.Type.IntArg), new CommandArg("yMap", CommandArg.Type.IntArg),
            new CommandArg("xPos", CommandArg.Type.IntArg, optional: true), new CommandArg("yPos", CommandArg.Type.IntArg, optional: true)));
        cs.AddCommand(new CommandDeclaration("goto", "Shifts Juni (absolute map+area coordinates)", null, false, ShiftCommand.NewGotoCommand, 
            new CommandArg("xMap", CommandArg.Type.IntArg), new CommandArg("yMap", CommandArg.Type.IntArg),
            new CommandArg("xPos", CommandArg.Type.IntArg, optional: true), new CommandArg("yPos", CommandArg.Type.IntArg, optional: true)));
        cs.AddCommand(new CommandDeclaration("exit", "Hides this console", null, false, ExitCommand.NewExitCommand));
        cs.AddCommand(new CommandDeclaration("quit", "Hides this console", null, true, ExitCommand.NewExitCommand));
        return cs;
    }

    public class SpeedCommand : ICommand
    {
        float? value;
        const float MIN_SPEED = 0.1f;
        const float MAX_SPEED = 5f;

        public SpeedCommand(CommandParseResult result)
        {
            if (result.Args["value"] == null) { value = null; return; }
            value = float.Parse(result.Args["value"], System.Globalization.CultureInfo.InvariantCulture.NumberFormat);
        }

        public static ICommand NewSpeedCommand(CommandParseResult result)
        {
            return new SpeedCommand(result);
        }

        public string Execute(object environment)
        {
            var env = (ConsoleExecutionEnvironment)environment;

            if (value == null)
            {
                env.Console.AddMessage($"Current speed: {GDKnyttDataStore.CurrentSpeed}");
                return null;
            }

            if (value < MIN_SPEED || value > MAX_SPEED) { return $"value should be between {MIN_SPEED} and {MAX_SPEED}"; }
            
            GDKnyttDataStore.CurrentSpeed = (float)value;
            env.Console.AddMessage($"Speed changed to {value}");

            return null;
        }
    }

    public class HelpCommand : ICommand
    {
        string name;

        public HelpCommand(CommandParseResult result)
        {
            name = result.Args["name"];
        }

        public static ICommand NewHelpCommand(CommandParseResult result)
        {
            return new HelpCommand(result);
        }

        public string Execute(object environment)
        {
            var env = (ConsoleExecutionEnvironment)environment;

            if (name == null)
            {
                env.Console.AddMessage($"Type list to see all commands, or help <command> to see help on a specific command");
                return null;
            }

            if (!env.Parser.Commands.Name2Command.ContainsKey(name))
            {
                return $"Cannot show help for unknown command: {name}";
            }

            var decl = env.Parser.Commands.Name2Command[name];
            env.Console.AddMessage($"{decl.DetailedDescription ?? decl.Description}");
            env.Console.AddMessage($"Usage: {decl}");

            return null;
        }
    }

    public class ListCommand : ICommand
    {
        public ListCommand(CommandParseResult result)
        {
        }

        public static ICommand NewListCommand(CommandParseResult result)
        {
            return new ListCommand(result);
        }

        public string Execute(object environment)
        {
            var env = (ConsoleExecutionEnvironment)environment;

            var list = env.Parser.Commands.MakeList(true);
            env.Console.AddMessage(string.Join("\n", list));

            return null;
        }
    }

    public class SaveCommand : ICommand
    {
        string subcmd;

        public SaveCommand(CommandParseResult result)
        {
            subcmd = result.Args["subcmd"];
        }

        public static ICommand NewSaveCommand(CommandParseResult result)
        {
            return new SaveCommand(result);
        }

        public string Execute(object environment)
        {
            var env = (ConsoleExecutionEnvironment)environment;
            var game = GDKnyttDataStore.Tree.Root.GetNodeOrNull<GDKnyttGame>("GKnyttGame");
            if (game == null) { return "No game is loaded"; }

            switch (subcmd)
            {
                case null:
                    game.saveGame(game.Juni, write: true);
                    break;
                case "print":
                    env.Console.AddMessage(game.GDWorld.KWorld.CurrentSave.ToString());
                    break;
                case "copy":
                    OS.Clipboard = game.GDWorld.KWorld.CurrentSave.ToString();
                    env.Console.AddMessage("Save file was copied to clipboard.");
                    break;
                case "paste":
                    KnyttSave save;
                    try
                    {
                        save = new KnyttSave(game.GDWorld.KWorld, OS.Clipboard, game.GDWorld.KWorld.CurrentSave.Slot);
                    }
                    catch (Exception)
                    {
                        return "Can't parse save file from clipboard";
                    }
                    game.GDWorld.KWorld.CurrentSave = save;
                    game.saveGame(save);
                    game.Juni.die();
                    break;
                default:
                    return "Can't recognize your command";
            }
            return null;
        }
    }

    public class SetCommand : ICommand
    {
        private static string[] names = Enum.GetNames(typeof(JuniValues.PowerNames));
        string variable;
        string value;

        public SetCommand(CommandParseResult result)
        {
            variable = result.Args["variable"];
            value = result.Args["value"];
        }

        public static ICommand NewSetCommand(CommandParseResult result)
        {
            return new SetCommand(result);
        }

        public string Execute(object environment)
        {
            var env = (ConsoleExecutionEnvironment)environment;
            var game = GDKnyttDataStore.Tree.Root.GetNodeOrNull<GDKnyttGame>("GKnyttGame");
            if (game == null) { return "No game is loaded"; }

            bool? bvalue = null;
            if (value == "on" || value == "1") { bvalue = true; }
            if (value == "off" || value == "0") { bvalue = false; }

            int power_index = Array.FindIndex(names, x => x.ToLower() == variable);
            if (power_index != -1 && bvalue != null)
            {
                game.Juni.setPower(power_index, bvalue ?? true);
                env.Console.AddMessage($"{((JuniValues.PowerNames)power_index).ToString()} set to {value}");
            }
            else if (variable.StartsWith("flag") && bvalue != null && 
                     int.TryParse(variable.Substring(4), out var flag_index) && flag_index >= 0 && flag_index < 10)
            {
                game.Juni.Powers.setFlag(flag_index, bvalue ?? true);
                game.UI.Location.updateFlags(game.Juni.Powers.Flags);
                env.Console.AddMessage($"Flag {flag_index} set to {value}");
            }
            else
            {
                return "Can't recognize variable or value";
            }
            return null;
        }
    }

    public class MonitorCommand : ICommand
    {
        string subcmd;

        public MonitorCommand(CommandParseResult result)
        {
            subcmd = result.Args["subcmd"];
        }

        public static ICommand NewMonitorCommand(CommandParseResult result)
        {
            return new MonitorCommand(result);
        }

        public string Execute(object environment)
        {
            var env = (ConsoleExecutionEnvironment)environment;
            var game = GDKnyttDataStore.Tree.Root.GetNodeOrNull<GDKnyttGame>("GKnyttGame");
            if (game == null) { return "No game is loaded"; }

            switch (subcmd)
            {
                case null:
                case "on":
                    game.UI.Location.Visible = true;
                    game.UI.Location.Flash = false;
                    game.UI.Location.showLabel();
                    break;
                case "flash":
                    game.UI.Location.Visible = true;
                    game.UI.Location.Flash = true;
                    game.UI.Location.showLabel();
                    break;
                case "flags":
                    game.UI.Location.ShowFlags = true;
                    game.UI.Location.Visible = true;
                    game.UI.Location.updateFlags(game.Juni.Powers.Flags);
                    break;
                case "off":
                    game.UI.Location.Visible = false;
                    break;
                default:
                    return "Can't recognize your command";
            }
            env.Console.AddMessage("Monitor parameters have been changed.");
            return null;
        }
    }

    public class ExitCommand : ICommand
    {
        public ExitCommand(CommandParseResult result) { }

        public static ICommand NewExitCommand(CommandParseResult result)
        {
            return new ExitCommand(result);
        }

        public string Execute(object environment)
        {
            GDKnyttDataStore.Tree.Root.GetNode<Console>("Console").toggleConsole();
            return null;
        }
    }

    public abstract class OnOffCommand : ICommand
    {
        string subcmd;

        public OnOffCommand(CommandParseResult result)
        {
            subcmd = result.Args["on"];
        }

        protected abstract void enable(bool on, GDKnyttGame game, ConsoleExecutionEnvironment env);

        public string Execute(object environment)
        {
            var env = (ConsoleExecutionEnvironment)environment;
            var game = GDKnyttDataStore.Tree.Root.GetNodeOrNull<GDKnyttGame>("GKnyttGame");
            if (game == null) { return "No game is loaded"; }

            if (subcmd == null || subcmd == "on")
            {
                enable(true, game, env);
            }
            else if (subcmd == "off")
            {
                enable(false, game, env);
            }
            else
            {
                return "Can't recognize your command";
            }
            return null;
        }
    }

    public class FlyCommand : OnOffCommand
    {
        public FlyCommand(CommandParseResult result) : base(result) { }

        public static ICommand NewFlyCommand(CommandParseResult result)
        {
            return new FlyCommand(result);
        }

        protected override void enable(bool on, GDKnyttGame game, ConsoleExecutionEnvironment env)
        {
            game.Juni.DebugFlyMode = on;
            env.Console.AddMessage(on ? "Now you can move Juni with arrow keys freely." : "Now Juni is in normal mode.");
        }
    }

    public class ImmuneCommand : OnOffCommand
    {
        public ImmuneCommand(CommandParseResult result) : base(result) { }

        public static ICommand NewImmuneCommand(CommandParseResult result)
        {
            return new ImmuneCommand(result);
        }

        protected override void enable(bool on, GDKnyttGame game, ConsoleExecutionEnvironment env)
        {
            game.Juni.Immune = on;
            env.Console.AddMessage(on ? "Now Juni is invulnerable." : "Now Juni is in normal mode.");
        }
    }

    public class MapCommand : OnOffCommand
    {
        public MapCommand(CommandParseResult result) : base(result) { }

        public static ICommand NewMapCommand(CommandParseResult result)
        {
            return new MapCommand(result);
        }

        protected override void enable(bool on, GDKnyttGame game, ConsoleExecutionEnvironment env)
        {
            game.UI.ForceMap = on;
            game.UI.GetNode<TouchPanel>("TouchPanel").InstallMap();
            game.UI.GetNode<InfoPanel>("InfoPanel").addItem("ItemInfo", (int)JuniValues.PowerNames.Map);
            game.UI.updatePowers();
            env.Console.AddMessage(on ? "Map is enabled. set map on: opens the whole map" : "Map is disabled (if level had no map).");
        }
    }

    public class ShiftCommand : ICommand
    {
        bool absolute;
        int x_map, y_map;
        int? x_pos, y_pos;

        public ShiftCommand(CommandParseResult result, bool absolute)
        {
            this.absolute = absolute;
            x_map = int.Parse(result.Args["xMap"]);
            y_map = int.Parse(result.Args["yMap"]);
            x_pos = result.Args.TryGetValue("xPos", out var i) && i != null ? (int?)int.Parse(i) : null;
            y_pos = result.Args.TryGetValue("yPos", out var j) && j != null ? (int?)int.Parse(j) : null;
        }

        public static ICommand NewShiftCommand(CommandParseResult result)
        {
            return new ShiftCommand(result, absolute: false);
        }

        public static ICommand NewGotoCommand(CommandParseResult result)
        {
            return new ShiftCommand(result, absolute: true);
        }

        public string Execute(object environment)
        {
            var env = (ConsoleExecutionEnvironment)environment;
            var game = GDKnyttDataStore.Tree.Root.GetNodeOrNull<GDKnyttGame>("GKnyttGame");
            if (game == null) { return "No game is loaded"; }

            var shift = new KnyttShift(game.CurrentArea.Area.Position, game.Juni.AreaPosition, KnyttSwitch.SwitchID.A);
            shift.AbsoluteTarget = absolute;
            shift.FormattedArea = new KnyttPoint(x_map, y_map);
            if (x_pos != null && y_pos != null) { shift.FormattedPosition = new KnyttPoint(x_pos.Value, y_pos.Value); }
            else { shift.RelativePosition = KnyttPoint.Zero; }

            if (!shift.RelativeArea.isZero()) { game.changeAreaDelta(shift.RelativeArea, true); }
            game.Juni.moveToPosition(game.CurrentArea, shift.AbsolutePosition);
            return null;
        }
    }
}
