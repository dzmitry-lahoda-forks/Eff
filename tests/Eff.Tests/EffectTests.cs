﻿using Nessos.Effects.Handlers;
using System;
using System.Threading.Tasks;
using Xunit;

namespace Nessos.Effects.Tests
{
    public static class EffectTests
    {
        public class TestEffect<T> : Effect<T>
        {

        }

        [Fact]
        public static void ConfigureAwait_ShouldAddCallerInfo()
        {
            var effect = new TestEffect<int>();
            var awaiter = effect.ConfigureAwait();

            Assert.True(awaiter.CallerMemberName?.Length > 0);
            Assert.True(awaiter.CallerFilePath?.Length > 0);
            Assert.True(awaiter.CallerLineNumber > 0);
        }

        public class TestEffect : Effect
        {

        }

        [Fact]
        public static void ConfigureAwait_Untyped_ShouldAddCallerInfo()
        {
            var eff = new TestEffect();
            var awaiter = eff.ConfigureAwait();

            Assert.True(awaiter.CallerMemberName?.Length > 0);
            Assert.True(awaiter.CallerFilePath?.Length > 0);
            Assert.True(awaiter.CallerLineNumber > 0);
        }
    }
}
