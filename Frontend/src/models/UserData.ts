export default class UserData {
  AccountId?: number;
  UserName?: string;
  Roles?: Array<string>;

  constructor(userDataJson: any) {
    if (userDataJson) {
      this.AccountId = userDataJson.accountId;
      this.UserName = userDataJson.userName;
      this.Roles = userDataJson.roles;
    }
  }

  hasRole(roleName: string): boolean {
    return this.Roles ? this.Roles.includes(roleName) : false;
  }

  hasRoles(roleNames: Array<string> | undefined): boolean {
    if (!roleNames) {
      return true;
    }
    return this.Roles ? this.Roles.some((x) => roleNames.includes(x)) : false;
  }
}
