import type { IWindowDescription } from "../../../interface/IWindowDescription";

export default class LoginWindowDescription implements IWindowDescription {
  title: string;

  constructor() {
    this.title = "Login";
  }
}
