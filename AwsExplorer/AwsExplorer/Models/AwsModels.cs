using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Data.Entity;
using System.Linq;
using System.Web;

namespace AwsExplorer.Models
{
  public class AwsEntities : DbContext
  {
    public DbSet<S3Object> S3Objects { get; set; }
  }

  public class S3Object
  {
    //
    // Summary:
    //     Gets and sets the Key property.
    [Required, Key]
    public string Key { get; set; }

    //
    // Summary:
    //     Gets and sets the ETag property.
    //public string ETag { get; set; }

    //
    // Summary:
    //     Gets and sets the LastModified property.  Date retrieved from S3 is in ISO8601
    //     format.  GMT formatted date is passed back to the user.
    public string LastModified { get; set; }

    //
    // Summary:
    //     Gets and sets the Owner property.
    //public Owner Owner { get; set; }

    //
    // Summary:
    //     Gets and sets the Size property.
    public long Size { get; set; }

    //
    // Summary:
    //     Gets and sets the StorageClass property.
    //public string StorageClass { get; set; }

    /// <summary>
    /// MIME type (not filled in list view)
    /// </summary>
    public string ContentType { get; set; }

    /// <summary>
    /// Content as Data URI (not filled in list view; only filled if content is image/png otherwise)
    /// </summary>
    public string Content { get; set; }
  }
}