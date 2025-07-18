import BaseService from "./BaseService";

export default class AccountService {
  static async GetLoggedInUser() {
    return BaseService.Get("/Account/GetLoggedInUser");
  }

  static async Login(LoginData) {
    return BaseService.Post("/Account/Login", LoginData);
  }

  static async Logout() {
    return BaseService.Post("/Account/Logout");
  }

  static async Register(RegisterData) {
    return BaseService.Post("/Account/Register", RegisterData);
  }
}
