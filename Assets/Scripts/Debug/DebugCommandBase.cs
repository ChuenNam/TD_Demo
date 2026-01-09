using System;
using UnityEngine;

public class DebugCommandBase
{
    private string _commandID;
    private string _commandDescription;
    private string _commandFormat;
    
    public string CommandID => _commandID;
    public string CommandDescription => _commandDescription;
    public string CommandFormat => _commandFormat;

    public DebugCommandBase(string id, string description, string format)
    {
        _commandID = id;
        _commandDescription = description;
        _commandFormat = format;
    }
}

public class DebugCommand : DebugCommandBase
{
    private Action _command;

    public DebugCommand(string id, string description, string format, Action command) : base(id, description, format)
    {
        _command = command;
    }
    public void Invoke()
    {
        _command.Invoke();
    }
}
public class DebugCommand<T> : DebugCommandBase
{
    private Action<T> _command;

    public DebugCommand(string id, string description, string format, Action<T> command) : base(id, description, format)
    {
        _command = command;
    }
    public void Invoke(T value)
    {
        _command.Invoke(value);
    }
}
public class DebugCommand<T1,T2> : DebugCommandBase
{
    private Action<T1,T2> _command;

    public DebugCommand(string id, string description, string format, Action<T1,T2> command) : base(id, description, format)
    {
        _command = command;
    }
    public void Invoke(T1 value1, T2 value2)
    {
        _command.Invoke(value1, value2);
    }
}
