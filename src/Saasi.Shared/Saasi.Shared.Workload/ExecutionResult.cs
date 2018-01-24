using System;

namespace Saasi.Shared.Workload {

    public sealed class ExecutionResult {
        public bool HasExceptions { get; set; }
        public DateTime TaskStartedAt { get; set; }
        public DateTime TaskFinishedAt { get; set; }
        public TimeSpan ActualExecutationTime { get { return TaskFinishedAt - TaskStartedAt; } }
        public string ThreadOfExecution {get; set;}
        public long ExecutedLoops { get; set; }
        public int ThreadsCount { get; set; }
        public string Payload { get; set; }
    } 
}