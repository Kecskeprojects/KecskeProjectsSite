import UserData from "./UserData";

export default class UserContext {
  user?: UserData;
  setUser?: (newUser: UserData) => void;

  constructor(user?: UserData, setUser?: (newUser: UserData) => void) {
    this.user = user;
    this.setUser = setUser;
  }
}
