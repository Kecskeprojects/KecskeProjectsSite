import { createContext } from "react";
import UserData from "./Models/UserData";

export const UserContext = createContext({
  user: new UserData(),
  setUser: (newUser = new UserData()) => {},
});
