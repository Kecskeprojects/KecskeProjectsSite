import { createContext } from "react";
import UserData from "../models/UserData";

export const UserContext = createContext({
  user: new UserData(undefined),
  setUser: (newUser: UserData) => {},
});
