using CommandLine;
using GraphManageGroup.Config;
using log4net;
using Microsoft.Graph;
using Microsoft.Identity.Client;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GraphManageGroup
{
    class Program
    {
        private static ILog logger = LogManager.GetLogger(typeof(Program));

        static void Main(string[] args)
        {
            var tokenHelper = new TokenHelper();
            //while (true)
            {
                bool failed = false;
                try
                {
                    //System.Net.WebRequest.DefaultWebProxy = new System.Net.WebProxy("127.0.0.1", 8888);
                    Parser.Default.ParseArguments<Options>(args).WithParsed<Options>(o =>
                    {
                        CheckArguments(o);
                        GroupManagement groupManagement = new GroupManagement(tokenHelper);
                        groupManagement.Run(o);
                    });
                }
                catch (Exception e)
                {
                    failed = true;
                    logger.Error($"An error occurred while run job, error: {e.ToString()}");
                }
                //if (!failed)
                //{
                //    break;
                //}
            }
            LogManager.Flush(10 * 1000);

        }

        private static void CheckArguments(Options option)
        {
            switch (option.Type)
            {
                case JobType.CreateGroup:
                    option.CheckGroupCount();
                    break;
                case JobType.CreateGroupAndAddOwnerAndMember:
                    option.CheckGroupName();
                    option.CheckGroupCount();
                    option.CheckOwnerCount();
                    option.CheckMemberCount();
                    break;
                case JobType.AddOwnerToGroup:
                    option.CheckGroupId();
                    option.CheckOwnerCount();
                    break;
                case JobType.AddMemberToGroup:
                    option.CheckGroupId();
                    option.CheckMemberCount();
                    break;
                case JobType.AddOwnerAndMemberToGroup:
                    option.CheckGroupId();
                    option.CheckOwnerCount();
                    option.CheckMemberCount();
                    break;
                case JobType.ChangeGroupsName:
                    option.CheckKeyWord();
                    option.CheckGroupName();
                    option.CheckGroupCount();
                    break;

            }
        }
    }
}
