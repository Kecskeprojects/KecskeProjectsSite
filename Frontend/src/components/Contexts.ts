import { createContext } from "react";
import UserContextModel from "../models/UserContextModel";

export const UserContext = createContext<UserContextModel>(
  new UserContextModel()
);
