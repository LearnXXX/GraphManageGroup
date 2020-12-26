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
        private Group TryGetGroupByDisplayName(string groupName)
        {
            string filterString = $"displayName  eq '{groupName}'";
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
                try
                {
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
                catch (Exception e)
                {
                    logger.Info($"Create group {tempGroupName} failed, error: {e.ToString()}");
                }
            }
        }
        private bool IsExistTeam(Group group)
        {
            try
            {
                groupService.Groups[group.Id].Team.Request().GetAsync().Wait();
            }
            catch (Exception e) when (e.InnerException != null && e.InnerException is ServiceException && ((ServiceException)e.InnerException).StatusCode == System.Net.HttpStatusCode.NotFound)
            {
                return false;
            }
            return true;
        }
        public void CreateTeamForGroups(Options option)
        {
            for (int index = 0; index < option.GroupCount; index++)
            {
                var tempGroupName = option.GroupName + index.ToString();
                var group = TryGetGroupByName(tempGroupName);
                if (group == null)
                {
                    logger.Info($"Start to create group {tempGroupName},{index}/{option.GroupCount}");
                    group = CreateGroup(tempGroupName);
                }
                if (!IsExistTeam(group))
                {
                    var team = new Team
                    {
                        MemberSettings = new TeamMemberSettings
                        {
                            AllowCreateUpdateChannels = true,
                            ODataType = null,
                        },
                        MessagingSettings = new TeamMessagingSettings
                        {
                            AllowUserEditMessages = true,
                            AllowUserDeleteMessages = true,
                            ODataType = null,
                        },
                        FunSettings = new TeamFunSettings
                        {
                            AllowGiphy = true,
                            GiphyContentRating = GiphyRatingType.Strict,
                            ODataType = null,
                        },
                        ODataType = null,
                    };
                    groupService.Groups[group.Id].Team.Request().PutAsync(team).Wait();
                    logger.Info($"Create team for group {tempGroupName}");
                }
            }
        }

        private List<Group> GetAllGroups()
        {
            List<Group> groups = new List<Group>();
            var currentPage = groupService.Groups.Request().GetAsync().Result;
            GetRequestAllOfDatas(currentPage, groups);
            return groups;
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
                case JobType.CreateTeamForGroups:
                    CreateTeamForGroups(option);
                    break;
                case JobType.ChangeGroupsName:
                    ChangeNameForGroups(option);
                    break;
                case JobType.RevertGroupsName:
                    RevertNameForGroups();
                    break;
            }
        }

        public void RevertNameForGroups()
        {
            var groups = GetAllGroups();
            foreach (var group in groups)
            {
                if (!string.Equals(group.DisplayName, group.MailNickname))
                {
                    try
                    {
                        var updateGroup = new Group { DisplayName = group.MailNickname };
                        groupService.Groups[group.Id].Request().UpdateAsync(updateGroup).Wait();
                        logger.Info($"Revert group name from {group.DisplayName} to {updateGroup.DisplayName}");
                    }
                    catch (Exception e)
                    {
                        logger.Warn($"An error occurred while revert group name from {group.DisplayName} to {group.MailNickname}, error: {e.ToString()}");
                    }

                }
            }
        }

        public void ChangeNameForGroups(Options option)
        {
            for (int index = 0; index < option.GroupCount; index++)
            {
                var tempGroupName = option.KeyWord + index.ToString();
                var group = TryGetGroupByDisplayName(tempGroupName);
                if (group != null)
                {
                    var updateGroup = new Group { DisplayName = $"{option.GroupName}{index}" };
                    groupService.Groups[group.Id].Request().UpdateAsync(updateGroup).Wait();
                    logger.Info($"Change group name from {group.DisplayName} to {updateGroup.DisplayName}");
                }
            }
            //    var groups = GetAllGroups();
            //var selectGroups = groups.FindAll(tempGroup => tempGroup.DisplayName.IndexOf(option.KeyWord, StringComparison.OrdinalIgnoreCase) >= 0);
            //int index = 1;
            //foreach (var group in selectGroups)
            //{
            //    var updateGroup = new Group { DisplayName = $"{option.GroupName}{index}" };
            //    groupService.Groups[group.Id].Request().UpdateAsync(updateGroup).Wait();
            //    logger.Info($"Change group name from {group.DisplayName} to {updateGroup.DisplayName}");
            //    index++;
            //}
        }

        private List<User> GetAllUsersWithouGuest()
        {
            var users = new List<User>();
            var select = "id,userPrincipalName,mail,userType";
            var currentPage = groupService.Users.Request().Select(select).GetAsync().Result;
            GetRequestAllOfDatas(currentPage, users);

            return users.Where(user => !string.Equals("Guest", user.UserType, StringComparison.OrdinalIgnoreCase)).ToList();
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
