using GraphManageGroup.Config;
using log4net;
using Microsoft.Graph;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GraphManageGroup
{
    class GroupManagement
    {
        private GraphServiceClient groupService;
        private List<User> allUsers;
        private static ILog logger = LogManager.GetLogger(typeof(GroupManagement));
        public GroupManagement(TokenHelper tokenHelper)
        {
            groupService = tokenHelper.GetGraphServiceClient();
        }

        private Group TryGetGroupByName(string groupName)
        {
            string filterString = $"MailNickname eq '{groupName}'";
            var groups = groupService.Groups.Request().Filter(filterString).GetAsync().Result;
            return groups.Count > 0 ? groups[0] : null;
        }

        public Group CreateGroup(string groupName)
        {
            var group = new Microsoft.Graph.Group
            {
                DisplayName = groupName,
                MailNickname = groupName,
                MailEnabled = true,
                GroupTypes = new string[] { "Unified" },
                SecurityEnabled = true
            };
            return groupService.Groups.Request().AddAsync(group).Result;
        }

        public void CreateMultiGroups(Options option)
        {
            for (int index = 0; index < option.GroupCount; index++)
            {
                var tempGroupName = option.GroupName + index.ToString();
                var users = this.AllofUsersWithOutGuest;
                var group = TryGetGroupByName(tempGroupName);
                if (group == null)
                {
                    logger.Info($"Start to create group {tempGroupName},{index}/{option.GroupCount}");
                    group = CreateGroup(tempGroupName);
                }

                if (option.Type == JobType.CreateGroupAndAddOwnerAndMember)
                {
                    AddMultiOwnersToGroup(group, option.OwnerCount);
                    AddMultiMembersToGroup(group, option.MemberCount);
                }
            }
        }


        public void Run(Options option)
        {
            switch (option.Type)
            {
                case JobType.CreateGroup:
                case JobType.CreateGroupAndAddOwnerAndMember:
                    CreateMultiGroups(option);
                    break;
                case JobType.AddMemberToGroup:
                    AddMultiMembersToGroup(new Group { Id = option.GroupId }, option.MemberCount);
                    break;
                case JobType.AddOwnerToGroup:
                    AddMultiOwnersToGroup(new Group { Id = option.GroupId }, option.OwnerCount);
                    break;
                case JobType.AddOwnerAndMemberToGroup:
                    AddMultiMembersToGroup(new Group { Id = option.GroupId }, option.MemberCount);
                    AddMultiOwnersToGroup(new Group { Id = option.GroupId }, option.OwnerCount);
                    break;
            }
        }

        private List<User> GetAllUsersWithouGuest()
        {
            var users = new List<User>();
            var select = "id,userPrincipalName,mail,userType";
            var currentPage = groupService.Users.Request().Select(select).GetAsync().Result;
            GetRequestAllOfDatas(currentPage, users);
            
            return users.Where(user =>!string.Equals("Guest", user.UserType,StringComparison.OrdinalIgnoreCase)).ToList();
        }

        /// <summary>
        /// Support for paging
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="currenPage"></param>
        /// <param name="datas"></param>
        /// <returns></returns>
        private List<T> GetRequestAllOfDatas<T>(dynamic currenPage, List<T> datas)
        {
            datas.AddRange(currenPage.CurrentPage as List<T>);
            if (currenPage.NextPageRequest != null)
            {
                var nextPage = currenPage.NextPageRequest.GetAsync().Result;
                GetRequestAllOfDatas(nextPage, datas);
            }
            return datas;
        }
        private List<User> AllofUsersWithOutGuest
        {
            get
            {
                if (allUsers == null)
                {
                    allUsers = GetAllUsersWithouGuest();
                }
                return allUsers;
            }
        }
        private Dictionary<string, DirectoryObject> GetGroupOwners(Group group)
        {
            var groupOwners = new List<DirectoryObject>();
            var currentPage = groupService.Groups[group.Id].Owners.Request().GetAsync().Result;
            GetRequestAllOfDatas(currentPage, groupOwners);
            return groupOwners.ToDictionary(key => key.Id, value => value, StringComparer.OrdinalIgnoreCase);
        }

        private Dictionary<string, DirectoryObject> GetGroupMembers(Group group)
        {
            var groupOwners = new List<DirectoryObject>();
            var currentPage = groupService.Groups[group.Id].Members.Request().GetAsync().Result;
            GetRequestAllOfDatas(currentPage, groupOwners);
            return groupOwners.ToDictionary(key => key.Id, value => value, StringComparer.OrdinalIgnoreCase);
        }

        private void AddUserToGroup(dynamic request, Dictionary<string, DirectoryObject> containUsers, int userCount)
        {
            if (containUsers.Count >= userCount)
            {
                return;
            }

            int index = containUsers.Count;
            foreach (var user in AllofUsersWithOutGuest)
            {
                if (!containUsers.ContainsKey(user.Id))
                {
                    request.AddAsync(user).Wait();
                    index++;
                }
                //logger.Info($"Add {user.DisplayName} successfully.");
                if (index >= userCount)
                { break; }
            }
        }

        public void AddMultiOwnersToGroup(Group group, int ownerCount)
        {
            logger.Info($"Start to add owners to group {group.Id}");
            var owners = GetGroupOwners(group);
            var ownerRequest = groupService.Groups[group.Id].Owners.References.Request();

            AddUserToGroup(ownerRequest, owners, ownerCount);
            logger.Info($"Finish add owners to group {group.Id}");
        }

        public void AddMultiMembersToGroup(Group group, int memberCount)
        {
            logger.Info($"Start to add members to group {group.Id}");
            var members = GetGroupMembers(group);
            var memberRequest = groupService.Groups[group.Id].Members.References.Request();
            AddUserToGroup(memberRequest, members, memberCount);
            logger.Info($"Finish add members to group {group.Id}");
        }


    }
}
