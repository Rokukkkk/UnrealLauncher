namespace UnrealLauncher.Core;

public enum ExecCode : ushort
{
    Success = 0,
    FileNotFound = 1001,
    SystemNotWindows = 1002,
    RegeditNotFound = 1003,
    PathIsNull = 1004,
    FileIsOccupying = 1005,
    FileAccessDenied = 1006,
    UnrealIsRunning = 1007
}

public struct Void;

public readonly struct ExecResult<T>(bool isSuccess, ExecCode execCode, T data)
{
    public bool IsSuccess { get; } = isSuccess;
    public ExecCode ExecCode { get; } = execCode;
    public T Data { get; } = data;

    public static ExecResult<T> Success(T data = default!) =>
        new(true, ExecCode.Success, data);

    public static ExecResult<T> Failed(ExecCode execCode) =>
        new(false, execCode, default!);
}