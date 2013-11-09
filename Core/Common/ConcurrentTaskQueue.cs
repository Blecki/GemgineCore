using System;
using System.Collections.Generic;
using System.Collections.Concurrent;
using System.Linq;
using System.Text;
using System.Threading;

namespace Gem.Common
{
    public enum ConcurrentTaskPauseResult
    {
        DidPause,
        DidNotPause,
    }

    public interface IConcurrentTask
    {
        void Execute(Func<ConcurrentTaskPauseResult> ContinueFunction);
    }

    public class GenericConcurrentTask : IConcurrentTask
    {
        Action<Func<ConcurrentTaskPauseResult>> Func;
        public GenericConcurrentTask(Action<Func<ConcurrentTaskPauseResult>> Func)
        {
            this.Func = Func;
        }

        void IConcurrentTask.Execute(Func<ConcurrentTaskPauseResult> ContinueFunction)
        {
            Func(ContinueFunction);
        }
    }

    public class IConcurrentCalculation : IConcurrentTask
    {
        protected virtual Object Calculate(Func<ConcurrentTaskPauseResult> ContinueFunction) { return null; }

        void IConcurrentTask.Execute(Func<ConcurrentTaskPauseResult> ContinueFunction)
        {
            Result = Calculate(ContinueFunction);
            Done = true;
        }

        public bool Done { get; private set; }
        public Object Result { get; private set; }
    }

    public class ConcurrentTaskQueue
    {
        private ConcurrentQueue<IConcurrentTask> PendingTasks = new ConcurrentQueue<IConcurrentTask>();

        public void EnqueueTask(IConcurrentTask Task)
        {
            PendingTasks.Enqueue(Task);
        }

        private Barrier PauseBarrier = new Barrier(1);
        private Barrier ResumeBarrier = new Barrier(1);
        List<Thread> WorkerThreads = new List<Thread>();
        private bool WaitingToPause = false;

        public void StartWorkerThreads(int ThreadCount)
        {
            PauseBarrier.AddParticipants(ThreadCount);
            ResumeBarrier.AddParticipants(ThreadCount);

            for (int I = 0; I < ThreadCount; ++I)
            {
                var Thread = new Thread(_workerThread);
                Thread.Start();
                WorkerThreads.Add(Thread);
            }
        }

        public void Stop()
        {
            foreach (var Thread in WorkerThreads)
                Thread.Abort();
        }

        public void Pause()
        {
            WaitingToPause = true;

            PauseBarrier.SignalAndWait();
        }

        public void Resume()
        {
            WaitingToPause = false;
            ResumeBarrier.SignalAndWait();
        }

        private void _workerThread()
        {
            try
            {
                while (true)
                {
                    if (this.WaitingToPause)
                    {
                        PauseBarrier.SignalAndWait();
                        ResumeBarrier.SignalAndWait();
                    }

                    IConcurrentTask WorkItem;
                    if (!PendingTasks.TryDequeue(out WorkItem))
                    {
                        Thread.Yield();
                        continue;
                    }

                    try
                    {
                        WorkItem.Execute(() =>
                            {
                                if (this.WaitingToPause)
                                {
                                    PauseBarrier.SignalAndWait();
                                    ResumeBarrier.SignalAndWait();
                                    return ConcurrentTaskPauseResult.DidPause;
                                }
                                else
                                    return ConcurrentTaskPauseResult.DidNotPause;

                            });
                    }
                    catch (ThreadAbortException)
                    {
                        return;
                    }
                    catch (Exception)
                    {
                        // System.Windows.Forms.MessageBox.Show(E.Message);
                    }

                }
            }
            catch (ThreadAbortException)
            {
                return;
            }
        }
    }
}
