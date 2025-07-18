export default class UserData {
  constructor(userDataJson) {
    this.AccountId = 0;
    this.UserName = "";
    this.Roles = [];

    if (userDataJson) {
      this.AccountId = userDataJson.accountId;
      this.UserName = userDataJson.userName;
      this.Roles = userDataJson.roles;
    }
  }

  isEmpty() {
    return !this.AccountId;
  }

  hasRole(roleName) {
    return this.Roles?.includes(roleName);
  }
}
