import { createContext } from "react";
import NotificationContextModel from "../models/NotificationContextModel";
import UserContextModel from "../models/UserContextModel";

export const UserContext = createContext<UserContextModel>(
  new UserContextModel()
);

export const NotificationContext = createContext<NotificationContextModel>(
  new NotificationContextModel()
);
