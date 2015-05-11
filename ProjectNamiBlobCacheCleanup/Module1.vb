Imports Microsoft.Azure.WebJobs
Imports Microsoft.WindowsAzure
Imports Microsoft.WindowsAzure.Storage
Imports Microsoft.WindowsAzure.Storage.Blob
Imports System.Configuration.ConfigurationManager

Module Module1
    Sub Main()
        Try
            'Set up connection to the cache
            Dim ThisStorageAccount As CloudStorageAccount = CloudStorageAccount.Parse("DefaultEndpointsProtocol=http;AccountName=" & System.Configuration.ConfigurationManager.AppSettings("ProjectNamiBlobCache.StorageAccount") & ";AccountKey=" & System.Configuration.ConfigurationManager.AppSettings("ProjectNamiBlobCache.StorageKey"))
            Dim ThisBlobClient As CloudBlobClient = ThisStorageAccount.CreateCloudBlobClient
            Dim ThisContainer As CloudBlobContainer = ThisBlobClient.GetContainerReference(System.Configuration.ConfigurationManager.AppSettings("ProjectNamiBlobCache.StorageContainer"))

            Try
                'Retrieve a list of blobs complete with metadata
                For Each ThisBlob As CloudBlockBlob In ThisContainer.ListBlobs(, , BlobListingDetails.Metadata)
                    Try
                        'If the blob is old, delete it
                        Dim LastModified As DateTimeOffset = ThisBlob.Properties.LastModified
                        If LastModified.UtcDateTime.AddSeconds(ThisBlob.Metadata("Projectnamicacheduration")) < DateTime.UtcNow Then 'Cache has expired
                            Console.Out.WriteLine("Deleting " & ThisBlob.Name)
                            ThisBlob.DeleteIfExists()
                        End If
                    Catch ex As Exception
                        'Delete any blob which causes an exception
                        Console.Out.WriteLine("ERROR (Blob " & ThisBlob.Name & ") - " & ex.Message & ex.StackTrace)
                        ThisBlob.DeleteIfExists()
                    End Try
                Next

            Catch ex As Exception
                Console.Out.WriteLine("ERROR - " & ex.Message & ex.StackTrace)
            End Try

        Catch ex As Exception
            Console.Out.WriteLine("ERROR - " & ex.Message & ex.StackTrace)
        End Try
    End Sub

End Module
