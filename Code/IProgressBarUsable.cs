using System;

 public  interface IProgressBarUsable
{
   public ulong CurrentProgress { get ; set ; }
    public ulong MaxProgress { get; set ; }
    public string ProgressBarName { get; }
    public Action<ulong, string?>? OnProgressUpdated { get ; set ; }


    void CalculateMaxProgress(string actionName);
  
}