﻿using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using Eff.Core;

namespace Eff.Examples.DependencyInjection
{
    public interface ILogger
    {
        void Log(string message);
    }

    public interface IUserService
    {
        Task<bool> Exists(string username);

        Task<bool> CreateUser(string username, string password);
    }

    public static class DomainLogic
    {
        // real-world enterprise application™
        // https://twitter.com/shit_sharp/status/1186637048335290368
        public static async Eff<string> CheckUsername(string username)
        {
            var userService = await Effect.GetDependency<IUserService>();

            if (await userService.Exists(username).AsEffect())
            {
                username = username + "1";

                if (await userService.Exists(username).AsEffect())
                {
                    // Two checks should be enough
                    username = username.Replace("1", "2");
                }
            }

            return username;
        }

        public static async Eff<bool> CreateNewUser(string userName, string password)
        {
            var userService = await Effect.GetDependency<IUserService>();
            var logger = await Effect.GetDependency<ILogger>();

            var newUserName = await CheckUsername(userName).AsEffect();

            if (await userService.CreateUser(newUserName, password).AsEffect())
            {
                logger.Log($"Successfully created user '{newUserName}'");
                return true;
            }
            else
            {
                logger.Log($"Failed to create user '{newUserName}'");
                return false;
            }
        }

        public static async Eff<int> CreateNewUsers((string userName, string password)[] credentials)
        {
            foreach (var cred in credentials)
            {
                await DomainLogic.CreateNewUser(cred.userName, cred.password).AsEffect();
            }

            return credentials.Length;
        }
    }
}
