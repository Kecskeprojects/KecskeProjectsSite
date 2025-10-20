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

  isEmpty(): boolean {
    return !this.AccountId;
  }

  hasRole(roleName: string): boolean {
    return this.Roles ? this.Roles.includes(roleName) : false;
  }
}
