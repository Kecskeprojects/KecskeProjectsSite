export default class UserData {
  AccountId?: number;
  UserName?: string;
  Roles?: Array<string>;

  constructor(userDataJson: any) {
    if (userDataJson?.content) {
      this.AccountId = userDataJson.content.accountId;
      this.UserName = userDataJson.content.userName;
      this.Roles = userDataJson.content.roles;
    }
  }

  hasRole(roleName: string): boolean {
    return this.Roles ? this.Roles.includes(roleName) : false;
  }

  hasRoles(roleNames?: Array<string>): boolean {
    if (!roleNames) {
      return true;
    }
    return this.Roles ? this.Roles.some((x) => roleNames.includes(x)) : false;
  }
}
