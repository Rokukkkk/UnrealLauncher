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

public struct ExecResult<T>
{
    public bool IsSuccess { get; }
    public ExecCode ExecCode { get; }
    public T Data { get; }

    private ExecResult(bool isSuccess, ExecCode execCode, T data)
    {
        IsSuccess = isSuccess;
        ExecCode = execCode;
        Data = data;
    }

    public static ExecResult<T> Success(T data = default!) =>
        new(true, ExecCode.Success, data);

    public static ExecResult<T> Failed(ExecCode execCode) =>
        new(false, execCode, default!);
}