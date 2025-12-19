import type ResponseObject from "../models/ResponseObject";
import BaseService from "./BaseService";

export default class SecurityService {
  static AddAddressToRule(ip?: string): Promise<ResponseObject> {
    const queryItems = {
      address: ip,
    };

    return BaseService.Post("/Security/AddAddressToRule", queryItems);
  }
}
