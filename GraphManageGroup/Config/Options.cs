using CommandLine;
using log4net;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GraphManageGroup.Config
{
    public enum JobType
    {
        CreateGroup = 0,
        CreateGroupAndAddOwnerAndMember = 1,
        AddOwnerToGroup = 2,
        AddMemberToGroup = 3,
        AddOwnerAndMemberToGroup = 4,
    }
    public class Options
    {
        [Option('t', "Type", Required = true, HelpText = "Input type for job(CreateGroup=0,CreateGroupAndAddOwnerAndMember=1,AddOwnerToGroup=2,AddMemberToGroup=3,AddOwnerAndMemberToGroup=4)")]
        public JobType Type { get; set; }

        [Option('n', "GroupName", Required = false, HelpText = "Input group name for CreateGroup job type")]
        public string GroupName { get; set; }

        [Option('i', "GroupId", Required = false, HelpText = "Input group id for add owner and member job type")]
        public string GroupId { get; set; }

        [Option('g', "GroupCount", Required = false, HelpText = "Input owner count for add owner job type")]
        public int GroupCount { get; set; }

        [Option('o', "OwnerCount", Required = false, HelpText = "Input owner count for add owner job type")]
        public int OwnerCount { get; set; }

        [Option('m', "MemberCount", Required = false, HelpText = "Input owner count for add member job type")]
        public int MemberCount { get; set; }

    }


    public static class OptionsExtention
    {
        private static ILog logger = LogManager.GetLogger(typeof(OptionsExtention));

        public static void CheckGroupName(this Options option)
        {
            if (string.IsNullOrEmpty(option.GroupName))
            {
                logger.Info("please input create group name:");
                option.GroupName = Console.ReadLine();
            }
        }

        public static void CheckOwnerCount(this Options option)
        {
            if (option.OwnerCount == 0)
            {
                logger.Info("please input add owner count:");
                option.OwnerCount = int.Parse(Console.ReadLine());
            }
        }

        public static void CheckMemberCount(this Options option)
        {
            if (option.MemberCount == 0)
            {
                logger.Info("please input add member count:");
                option.MemberCount = int.Parse(Console.ReadLine());
            }
        }

        public static void CheckGroupCount(this Options option)
        {
            if (option.GroupCount == 0)
            {
                logger.Info("please input create group count:");
                option.GroupCount = int.Parse(Console.ReadLine());
            }
        }


        public static void CheckGroupId(this Options option)
        {
            if (string.IsNullOrEmpty(option.GroupId))
            {
                logger.Info("please input group id:");
                option.GroupId = Console.ReadLine();
            }
        }

    }
}
