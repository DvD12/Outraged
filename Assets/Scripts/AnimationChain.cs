using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Outraged
{
    public struct AnimationStep
    {
        public int DurationMS;
        public Action<DateTime, DateTime, DateTime> Update;
        public Action OnStepStart;
        public Action OnStepEnd;
        private string Name;
        public AnimationStep(int ms, Action<DateTime, DateTime, DateTime> action, string name, Action onStepStart = null, Action onStepEnd = null)
        {
            DurationMS = ms;
            Update = action;
            Name = name;
            OnStepStart = onStepStart;
            OnStepEnd = onStepEnd;
        }
    }
    public class AnimationChain
    {
        private List<AnimationStep> Steps = new List<AnimationStep>();
        private DateTime StartTime;
        private bool IsRepeatable = false;
        private bool HasStarted = false;
        private Action OnStop;

        public AnimationChain(bool repeat, Action onStop = null)
        {
            this.IsRepeatable = repeat;
            this.OnStop = onStop;
        }

        public void Add(int ms, Action<DateTime, DateTime, DateTime> update, string name = "", Action onStepStart = null, Action onStepEnd = null) => Steps.Add(new AnimationStep(ms, update, name, onStepStart, onStepEnd));

        public void Start()
        {
            StartTime = DateTime.Now;
            HasStarted = true;
        }
        public void Stop()
        {
            if (HasStarted)
            {
                OnStop?.Invoke();
                HasStarted = false;
            }
        }
        private bool IsUpdating = false;
        public void Update() // Only allow one Update() to run at any given time to avoid jerk
        {
            if (!HasStarted) { return; }
            if (!IsUpdating)
            {
                IsUpdating = true;
                //if (!HasStarted) { Start(); }
                DateTime currentStepStart = StartTime;
                DateTime currentStepEnd = StartTime;
                DateTime now = DateTime.Now;
                int CurrentStep = -1;
                for (int i = 0; i < Steps.Count; i++)
                {
                    var Step = Steps[i];
                    currentStepEnd = currentStepEnd.AddMilliseconds(Step.DurationMS);
                    if (now < currentStepEnd)
                    {
                        var oldStep = CurrentStep;
                        CurrentStep = i;
                        if (oldStep != CurrentStep)
                        {
                            if (i > 0 && Steps[i - 1].OnStepEnd != null)
                            {
                                Steps[i - 1].OnStepEnd();
                            }
                            Step.OnStepStart?.Invoke();
                        }
                        Step.Update(currentStepStart, currentStepEnd, now);
                        break;
                    }
                    else if (i == Steps.Count - 1) // Time elapsed -> Animation is over
                    {
                        if (IsRepeatable) { StartTime = now; }
                        else { Stop(); }
                    }
                    currentStepStart = currentStepStart.AddMilliseconds(Step.DurationMS); // The beginning of the next step is the current step + current delta
                }
                IsUpdating = false;
            }
        }
    }
}
