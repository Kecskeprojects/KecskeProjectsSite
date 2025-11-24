import type { IWindowDescription } from "../../../interface/IWindowDescription";
import AccountService from "../../../services/AccountService";

export default class LoginWindowDescription implements IWindowDescription {
  title: string;
  serviceFunction: (data: FormData) => Promise<any>;

  constructor() {
    this.title = "Login";
    this.serviceFunction = AccountService.Login;
  }
}
