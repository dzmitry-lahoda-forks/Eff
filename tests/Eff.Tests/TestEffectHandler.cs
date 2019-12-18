﻿#pragma warning disable 1998

using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Nessos.Eff.Tests
{
    public class TestEffectHandler : EffectHandler
    {
        private readonly DateTime now;

        public List<ExceptionLog> ExceptionLogs { get; }
        public List<ResultLog> TraceLogs { get; }

        public TestEffectHandler(DateTime now) : base()
        {
            this.now = now;
            ExceptionLogs = new List<ExceptionLog>();
            TraceLogs = new List<ResultLog>();
        }

        public TestEffectHandler() : this(DateTime.Now)
        { }

        public override async Task Handle<TResult>(IEffect<TResult> effect)
        {
            switch (effect)
            {
                case DateTimeNowEffect dateTimeNowEffect:
                    dateTimeNowEffect.SetResult(now);
                    break;
                case FuncEffect<TResult> funcEffect:
                    var result = funcEffect.Func();
                    effect.SetResult(result);
                    break;
            }
        }

        public (string name, object value)[] CaptureStateParameters { private set; get; }
        public (string name, object value)[] CaptureStateLocalVariables { private set; get; }
        public override async Task Handle<TResult>(TaskEffect<TResult> effect)
        {
            CaptureStateParameters = TraceHelpers.GetParametersValues(effect.State);
            CaptureStateLocalVariables = TraceHelpers.GetLocalVariablesValues(effect.State);

            try
            {
                var result = await effect.Task;
                effect.SetResult(result);
                await Log(result, effect);
            }
            catch (Exception ex)
            {
                await Log(ex, effect);
                throw;
            }

        }

        public override async Task Handle<TResult>(EffEffect<TResult> effect)
        {
            try
            {
                var result = await effect.Eff.Run(this);
                effect.SetResult(result);
                await Log(result, effect);
            }
            catch (Exception ex)
            {
                await Log(ex, effect);
                throw;
            }
        }

        public async ValueTask<ValueTuple> Log(Exception ex, IEffect effect)
        {
            var log =
                new ExceptionLog
                {
                    CallerFilePath = effect.CallerFilePath,
                    CallerLineNumber = effect.CallerLineNumber,
                    CallerMemberName = effect.CallerMemberName,
                    Exception = ex,
                    Parameters = TraceHelpers.GetParametersValues(effect.State),
                    LocalVariables = TraceHelpers.GetLocalVariablesValues(effect.State),
                };
            ExceptionLogs.Add(log);

            if (!ex.Data.Contains("StackTraceLog"))
            {
                var queue = new Queue<ExceptionLog>();
                queue.Enqueue(log);
                ex.Data["StackTraceLog"] = queue;

                return ValueTuple.Create();
            }

            ((Queue<ExceptionLog>)ex.Data["StackTraceLog"]).Enqueue(log);

            return ValueTuple.Create();
        }

        public async ValueTask<ValueTuple> Log(object result, IEffect effect)
        {
            var log =
                new ResultLog
                {
                    CallerFilePath = effect.CallerFilePath,
                    CallerLineNumber = effect.CallerLineNumber,
                    CallerMemberName = effect.CallerMemberName,
                    Result = result,
                    Parameters = TraceHelpers.GetParametersValues(effect.State),
                    LocalVariables = TraceHelpers.GetLocalVariablesValues(effect.State),
                };
            TraceLogs.Add(log);
            return ValueTuple.Create();
        }
    }


}
