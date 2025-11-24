import axios, {
  AxiosError,
  AxiosHeaders,
  type AxiosProgressEvent,
} from "axios";
import Constants from "../enum/Constants";
import type ResponseObject from "../models/ResponseObject";
import BackendServiceTools from "../tools/BackendServiceTools";
import EnvironmentTools from "../tools/EnvironmentTools";
import LogTools from "../tools/LogTools";

export default class BaseService {
  static async Get(
    route: string,
    queryItems?: any,
    additionalHeaders?: AxiosHeaders
  ): Promise<ResponseObject> {
    return BaseService.BaseFetch(
      "GET",
      route,
      queryItems,
      undefined,
      undefined,
      undefined,
      additionalHeaders
    );
  }

  static async Post(
    route: string,
    queryItems?: any,
    body?: FormData,
    onUploadProgress?: (progressEvent: AxiosProgressEvent) => void,
    onDownloadProgress?: (progressEvent: AxiosProgressEvent) => void,
    additionalHeaders?: AxiosHeaders
  ): Promise<ResponseObject> {
    return BaseService.BaseFetch(
      "POST",
      route,
      queryItems,
      body,
      onUploadProgress,
      onDownloadProgress,
      additionalHeaders
    );
  }

  static async Put(
    route: string,
    queryItems?: any,
    body?: FormData,
    additionalHeaders?: AxiosHeaders
  ): Promise<ResponseObject> {
    return BaseService.BaseFetch(
      "PUT",
      route,
      queryItems,
      body,
      undefined,
      undefined,
      additionalHeaders
    );
  }

  static async Delete(
    route: string,
    queryItems?: any,
    body?: FormData,
    additionalHeaders?: AxiosHeaders
  ): Promise<ResponseObject> {
    return BaseService.BaseFetch(
      "DELETE",
      route,
      queryItems,
      body,
      undefined,
      undefined,
      additionalHeaders
    );
  }

  static async BaseFetch(
    method: string,
    route: string,
    queryItems?: any,
    body?: FormData,
    onUploadProgress?: (progressEvent: AxiosProgressEvent) => void,
    onDownloadProgress?: (progressEvent: AxiosProgressEvent) => void,
    additionalHeaders?: AxiosHeaders
  ): Promise<ResponseObject> {
    if (!route.startsWith("/") && !route.startsWith("\\")) {
      route = "/" + route;
    }

    const query = BackendServiceTools.BuildQuery(queryItems);

    let responseBody;
    try {
      const response = await axios(
        `${EnvironmentTools.getBackendRoute()}${route}${query}`,
        {
          method: method,
          withCredentials: !EnvironmentTools.IsProduction(),
          headers: additionalHeaders,
          data: body,
          validateStatus: (status) => status < 400,
          onDownloadProgress: onDownloadProgress,
          onUploadProgress: onUploadProgress,
        }
      );

      responseBody = response?.data as ResponseObject;
    } catch (error) {
      if (axios.isAxiosError(error)) {
        BaseService.ErrorHandling(route, error);

        responseBody = error.response?.data as ResponseObject;
      } else {
        //Todo: Proper user friendly error handling using floating popups or something
        console.log("Non Axios Error!");
        console.log(error);

        responseBody = {
          error: "Unexpected error before request!",
        } as ResponseObject;
      }
    }

    if (!EnvironmentTools.IsProduction() && responseBody) {
      LogTools.DebugLog("Response Body:", responseBody);
    }

    return responseBody;
  }

  static ErrorHandling(route: string, errorData: AxiosError<any, any>): void {
    if (!EnvironmentTools.IsProduction()) {
      if (errorData.response) {
        // The request was made and the server responded with a status code
        console.log("[Error] Response From Backend:");
        console.log(errorData.response.data);
        console.log(errorData.response.status);
      } else if (errorData.request) {
        // The request was made but no response was received
        // `error.request` is an instance of XMLHttpRequest
        console.log("[Error] No Response From Backend:");
        console.log(errorData.request);
      } else {
        // Something happened in setting up the request that triggered an Error
        console.log("[Error] Pre-Request Configuration Issue:");
        console.log(errorData.message);
      }
    }

    //Todo: Proper user friendly error handling using floating popups or something
    const status = errorData?.response?.status
      ? errorData?.response?.status
      : errorData?.request?.status;
    const response = errorData?.response?.data as ResponseObject;
    if (response?.error) {
      console.log(`Status Code: ${status}\nError: ${response.error}`);
    }

    if (
      (status === 401 || status === 403) &&
      !route.startsWith(Constants.GetUserStateEndpoint) &&
      !route.startsWith(Constants.LoginEndpoint)
    ) {
      window.location.replace(Constants.LoginRoute);
    }
  }
}
