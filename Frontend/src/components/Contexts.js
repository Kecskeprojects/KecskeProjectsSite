import { createContext } from "react";
import UserData from "../models/UserData";

export const UserContext = createContext({
  user: new UserData(),
  setUser: (newUser = new UserData()) => {},
});
