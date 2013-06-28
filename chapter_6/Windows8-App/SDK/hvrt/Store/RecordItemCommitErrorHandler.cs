using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml;
using System.Net;
using System.Net.Http;
using HealthVault.Foundation;

namespace HealthVault.Store
{
    /// <summary>
    /// This class abstracts out the error handling checks used by RecordItemCommitErrorHandler
    ///  - You can unit test this code
    ///  - You could, if necessary, override this logic in the future
    /// </summary>
    public sealed class RecordItemCommitErrorHandler
    {
        public RecordItemCommitErrorHandler()
        {
        }

        public int MaxAttemptsPerChange
        {
            get; set;
        }

        public bool IsHaltingError(Exception ex)
        {
            if (this.IsServerError(ex) ||
                this.IsAccessDeniedError(ex) ||
                this.IsClientError(ex)
               )
            {
                return true;
            }

            if (this.IsHttpError(ex) && !HealthVaultApp.HasInternetAccess())
            {
                return true;
            }

            return false;
        }

        public bool ShouldRetryCommit(RecordItemChange change, Exception ex)
        {
            if (this.IsClientValidationError(ex) ||
                this.IsSerializationError(ex) ||
                !this.IsHttpError(ex)
            )
            {
                return false;
            }
            
            if (this.MaxAttemptsPerChange > 0 && change.Attempt >= this.MaxAttemptsPerChange)
            {
                return false;
            }

            return true;
        }

        public bool ShouldCreateNewItemForConflict(RecordItemChange change, Exception ex)
        {
            ServerException se = ex as ServerException;
            if (se != null)
            {
                return (this.IsItemKeyNotFound(se)); // Sometimes when the item is really missing, we get this error..
            }

            return false;
        }
        
        public bool IsItemKeyNotFound(Exception ex)
        {
            ServerException se = ex as ServerException;
            if (se != null)
            {
                return (this.IsItemKeyNotFound(se)); 
            }

            return false;
        }

        internal bool IsItemKeyNotFound(ServerException se)
        {
            return (se.IsItemNotFound || 
                    se.IsVersionStampMismatch ||
                    se.IsStatusCode(ServerStatusCode.InvalidXml)); // And sometimes the platform will return InvalidXml when an ItemKey was not found
        }

        public bool IsClientValidationError(Exception ex)
        {
            return ((ex is ArgumentException)  ||
                    (ex is ArgumentNullException));
        }

        public bool IsSerializationError(Exception ex)
        {
            return (ex is XmlException);
        }

        public bool IsAccessDeniedError(Exception ex)
        {
            ServerException se = ex as ServerException;
            if (se != null)
            {
                return se.IsAccessDenied;
            }

            return false;
        }

        public bool IsClientError(Exception ex)
        {
            return (ex is ClientException);
        }

        public bool IsHttpError(Exception ex)
        {
            WebException webEx = ex as WebException;
            if (webEx == null)
            {
                HttpRequestException re = ex as HttpRequestException;
                if (re != null)
                {
                    webEx = re.InnerException as WebException;
                    if (webEx == null)
                    {
                        return true; // Some other random HttpException...
                    }
                }
            }
        
            if (webEx == null)
            {
                return false;
            }

            return this.ShouldRetryHttpError(webEx.Status); 
        }
        
        public bool IsServerError(Exception ex)
        {
            ServerException se = ex as ServerException;
            if (se != null)
            {
                return se.IsServerError;
            }

            return false;
        }

        internal bool ShouldRetryHttpError(WebExceptionStatus status)
        {
            switch (status)
            {
                default:
                    break;

                case WebExceptionStatus.RequestCanceled:
                case WebExceptionStatus.MessageLengthLimitExceeded:
                    return false;
            }

            return true;
        }

    }
}
