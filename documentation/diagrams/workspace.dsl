
workspace "Azure Data Platform Reference Architecture" {

    model {
       

            enterprise "XYZ Pty Ltd" {
                InformationConsumer = person "Information Consumer" "" ""
                InformationProducer = person "Information Producer" "" ""                
                

                group "ClientDevices" {
                    cd = softwaresystem "Client Web Browsers" "" "Web Browser" {
                        infcbrowser = container "Web Browser" "" "" "Web Browser"
                    }
                }
        
                group "Azure" {
                    
                    #Enterprise X
                    pbiaudcap = softwaresystem "Power BI Audit Capture Solution"{
                        pbiaudstor = container "Audit Files - Raw" "..." "...." "Microsoft Azure - Storage Accounts"                        
                        pbiaudwebapp = container "Web Application" "..." "...." "Microsoft Azure - Web Environment"
                        pbiaudwebappds = container "Web Application - Configuration" "..." "...." "Microsoft Azure - SQL Database"                        
                        pbiaudwebappdsaut = container "AutoUpdater for Web App Configuration" "..." "...." "Microsoft Azure - Function Apps"
                    }

                    pbiutcap = softwaresystem "Power BI Metadata & Telemetry Capture Solution"{                        
                        pbimetastor = container "Power BI Metadata & Usage" "..." "...." "Microsoft Azure - Storage Accounts"                                        
                        pbimetafunc = container "Power BI Metadata Collection Functions" "..." "...." "Microsoft Azure - Function Apps" 
                                                                        
                        
                    }

                    pbiaudproc = softwaresystem "Power BI Audit Processing Solution"{
                        pbiaudstorp = container "Audit Files - Processed" "..." "...." "Microsoft Azure - Storage Accounts"                      
                        pbiaudtfunc = container "Post Processing Function" "..." "...." "Microsoft Azure - Function Apps"
                        pbiaudxfunc = container "Audit File Generation Function" "..." "...." "Microsoft Azure - Function Apps"
                        
                    }

                    pbipp = softwaresystem "Power Platform" "" "PowerBI"{
                       pbiaudpbi = container "PowerBi Service - app.powerbi.com" "...." ""  "PowerBI"                        
                       pbiaudpbiapi = container "PowerBi Admin Api" "...." ""  "PowerBI"
                       pbiembeddedapi = container "PowerBi Embedded Api" "...." ""  "PowerBI"                        
                    }

                    entaud = softwaresystem "Enterprise Audit Storage & Analytics" "" ""{
                        edlaud = container "Enterprise Data Lake for Audit Records" "...." ""  "Microsoft Azure - Storage Accounts"   
                    }

                    
                }

            }
    
            # relationships between people and software systems
            infcbrowser -> InformationConsumer
            pbiaudwebapp -> infcbrowser "Response"
            infcbrowser -> pbiaudwebapp "Request"
            pbiaudwebapp -> pbiaudpbi "Request"
            pbiaudpbi -> pbiaudwebapp "Response"
            pbiaudwebapp -> pbiaudstor "Complex Response JSON"
            pbiaudwebapp -> pbiaudwebappds
            pbiaudwebappds -> pbiaudwebapp
            pbiaudstor -> pbiaudtfunc
            pbiaudtfunc ->  pbiaudstorp
            pbiaudstorp -> pbiaudxfunc
            pbiaudxfunc -> edlaud
            InformationProducer -> pbiaudwebapp "Manage Application Configuration"
            pbiaudpbiapi -> pbimetafunc
            pbimetafunc -> pbimetastor
            pbimetastor -> pbiaudwebappdsaut
            pbiaudwebappdsaut -> pbiaudwebappds


    }
    
    views {
        systemLandscape "SystemLandscape" {
            include *
            autoLayout lr
        }

        container pbiaudcap "AuditCapatureContainerView"{
            include *
            #autoLayout

        }

        container pbiutcap "MetadataCaptureContainerView"{
            include *
            autoLayout

        }

        container pbiaudproc "AuditProcessingContainerView"{
            include *
            autoLayout

        }

        container entaud "EnterpriseAuditContainerView"{
            include *
            autoLayout

        }

        container pbipp "PowerPlatformContainerView"{
            include *
            autoLayout

        }




    
    
        styles {
            element "Person" {
                color #ffffff
                fontSize 22
                shape Person
            }
            element "DataEngineer" {
                background #08427b
            }
            element "Bank Staff" {
                background #999999
            }
            element "Software System" {
                background #FFFFFF
            }
            
            element "Container" {
                background #f7f7f7
                #color #ffffff
            }
            element "Web Browser" {
                shape WebBrowser
            }
            element "Mobile App" {
                shape MobileDeviceLandscape
            }
            element "Database" {
                shape Cylinder
            }
            element "Component" {
                background #85bbf0
                color #000000
            }
            

            element "Failover" {
                opacity 25
            }
            element "PowerBI" {
                icon icons/PowerBI-Icon-Transparent.png
            }
            element "Purview" {
                icon icons/azure-purview-logo.png
            }

            element "OnPrem" {
                icon icons/enterprise_building.png
            }

            element "Lockbox" {
                icon icons/Resource-Groups.png
                stroke #424549
            }

             element "adsgofast" {
                icon icons/ads_go_fast.png
                stroke #424549
            }

            element "datashare" {
                icon icons/Data-Share.png
            }


            relationship "BlockedFlow" {
                color #DE2D1F
                colour #DE2D1F
                dashed false
            }

            relationship "AuditFlow" {
                color #0000FF
                colour #0000FF
                dashed false
            }

            relationship "AccessFlow" {
                color #228B22 
                colour #228B22 
                dashed false
            }

            relationship "DataFlow" {
                color #D1D100 
                colour #D1D100 
                dashed false
            }

        }                               
        
        themes https://static.structurizr.com/themes/microsoft-azure-2021.01.26/theme.json
        
    }
}


