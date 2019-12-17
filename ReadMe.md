# 								UserGuide

**CommandLine:**

1.Only create multiple groups

​	a. GraphManageGroup.exe --Type CreateGroup --GroupName  **GroupName** --GroupCount **10**

​	b. GraphManageGroup.exe -t 0 -n **GroupName**  -g **10**



2.Create multiple groups and add multiple owners and members

​	a. GraphManageGroup.exe --Type CreateGroupAndAddOwnerAndMember --GroupName  **GroupName** --GroupCount **10** --OwnerCount **10** --MemberCount **10**

​	b. GraphManageGroup.exe -t 1 -n **GroupName**  -g **10** -o **10**  -m **10**

*this tool will add owner and member  to group automatically, no need config user name*



3.Add multiple owners to group

​	a. GraphManageGroup.exe --Type AddOwnerToGroup --GroupId **GroupId** --OwnerCount **10**

​	b. GraphManageGroup.exe -t 2 -i **GroupId** -o **10** 



4.Add multiple members to group

​	a. GraphManageGroup.exe --Type AddMemberToGroup --GroupId **GroupId** --MemberCount **10**

​	b. GraphManageGroup.exe -t 3 -i **GroupId** -m **10**



5.Add multiple owners and members to group

​	a.  GraphManageGroup.exe --Type AddOwnerAndMemberToGroup --GroupId **GroupId** --OwnerCount **10** --MemberCount **10**

​	b. GraphManageGroup.exe -t 4 -i **GroupId** -m **10** -o **10** 