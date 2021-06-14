```mermaid
sequenceDiagram
    autonumber
    participant Web Browser
    %% participant PowerBI Auditor
    participant PowerBI Embedded
    participant Configuration File
    participant PowerBI.com
    participant AAD
    participant Cube (Data)
    Web Browser ->> PowerBI Embedded: Request Report List?
    AAD ->> PowerBI Embedded: Get User Details
    PowerBI Embedded ->> PowerBI.com : Check User Rights
    PowerBI Embedded ->> Web Browser: Return Report List?
    Web Browser ->> PowerBI.com: Request Specific Report?
    AAD ->> PowerBI.com: Get User Details
    PowerBI.com ->> PowerBI.com : Check User Rights
    Cube (Data) ->> PowerBI.com : Get Data with User Context 
    PowerBI.com ->> Web Browser: Return Report & Data?
    