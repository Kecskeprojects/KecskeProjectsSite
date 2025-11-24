import Constants from "../enum/Constants";
import type ResponseObject from "../models/ResponseObject";
import UserData from "../models/UserData";
import BaseService from "./BaseService";

export default class AccountService {
  static async GetLoggedInUser(): Promise<UserData> {
    const data = await BaseService.Get(Constants.GetUserStateEndpoint);
    return new UserData(data?.content);
  }

  static async Login(LoginData: FormData): Promise<UserData> {
    const data = await BaseService.Post(
      Constants.LoginEndpoint,
      null,
      LoginData
    );
    return new UserData(data);
  }

  static async Logout(): Promise<ResponseObject> {
    return BaseService.Post("/Account/Logout");
  }

  static async Register(RegisterData: FormData): Promise<ResponseObject> {
    return BaseService.Post("/Account/Register", null, RegisterData);
  }
}
