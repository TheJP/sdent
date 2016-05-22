
using System;
using System.Runtime.ConstrainedExecution;

public class GuiCommand
{
    public enum Row
    {
        Top,
        Middle,
        Bottom,
        Any
    }

    public enum Column
    {
        Q,
        W,
        E,
        R,
        T,
        Any,
    }

    private IAbility ability;

    public Row PreferredRow { get; set; }
    public Column PreferredColumn { get; set; }

    public String ToolTip { get; set; }
    public bool Enabled { get; set; }

    public GuiCommand(IAbility ability, Row preferredRow = Row.Any, Column preferredColumn = Column.Any)
    {
        this.ability = ability;
        PreferredRow = preferredRow;
        PreferredColumn = preferredColumn;
    }
}