import type { IWindowDescription } from "./IWindowDescription";

export default class LoginWindowDescription implements IWindowDescription {
  title: string;

  constructor() {
    this.title = "Login";
  }
}
