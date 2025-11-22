import Constants from "../enum/Constants";
import UserData from "../models/UserData";
import BaseService from "./BaseService";

export default class AccountService {
  static async GetLoggedInUser(): Promise<UserData> {
    const data = await BaseService.Get(Constants.GetUserStateEndpoint);
    return new UserData(data);
  }

  static async Login(LoginData: FormData): Promise<UserData> {
    const data = await BaseService.Post(
      Constants.LoginEndpoint,
      null,
      LoginData
    );
    return new UserData(data);
  }

  //Todo: Define return type
  static async Logout(): Promise<any> {
    return BaseService.Post("/Account/Logout");
  }

  //Todo: Define return type
  static async Register(RegisterData: FormData): Promise<any> {
    return BaseService.Post("/Account/Register", null, RegisterData);
  }
}
