export default class UserData {
  AccountId: number;
  UserName: string;
  Roles: Array<string>;

  constructor(userDataJson: any) {
    this.AccountId = 0;
    this.UserName = "";
    this.Roles = [];

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
    return this.Roles?.includes(roleName);
  }
}
