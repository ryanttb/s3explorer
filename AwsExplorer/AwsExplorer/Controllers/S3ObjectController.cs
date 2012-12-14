using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Entity;
using System.Linq;
using System.Web;
using System.Web.Configuration;
using System.Web.Mvc;
using AwsExplorer.Models;
using Amazon.S3;
using ICSharpCode.SharpZipLib.Zip;
using System.IO;

namespace AwsExplorer.Controllers
{
  public class S3ObjectController : Controller
  {
    private AwsEntities db = new AwsEntities();
    private AmazonS3 s3 = Amazon.AWSClientFactory.CreateAmazonS3Client(new AmazonS3Config());

    //
    // GET: /S3Object/

    public ActionResult Index(string prefix = "", int maxKeys = 100)
    {
      ViewBag.prefix = prefix;
      ViewBag.maxKeys = maxKeys;

      var list = new Amazon.S3.Model.ListObjectsRequest();
      list.WithBucketName(WebConfigurationManager.AppSettings["UploadBucket"]);
      if (!String.IsNullOrWhiteSpace(prefix))
      {
        list.WithPrefix(prefix);
      }

      list.WithMaxKeys(maxKeys);

      var response = s3.ListObjects(list);

      return View(response.S3Objects.Select(e => new S3Object
        {
          Key = e.Key,
          Size = e.Size,
          LastModified = e.LastModified
        }));
    }

    //
    // GET: /S3Object/Details/5

    public ActionResult Details(string id = null, string prefix = "", int maxKeys = 100)
    {
      ViewBag.prefix = prefix;
      ViewBag.maxKeys = maxKeys;

      var list = new Amazon.S3.Model.ListObjectsRequest();
      list.WithBucketName(WebConfigurationManager.AppSettings["UploadBucket"]);
      list.WithPrefix(id);

      var s3Objects = s3.ListObjects(list).S3Objects;
      if (s3Objects.Count == 0)
      {
        return HttpNotFound();
      }

      var get = new Amazon.S3.Model.GetObjectRequest();
      get.WithBucketName(WebConfigurationManager.AppSettings["UploadBucket"]);
      get.WithKey(s3Objects[0].Key);

      var response = s3.GetObject(get);

      S3Object modelObject = new S3Object
      {
        Key = s3Objects[0].Key,
        Size = s3Objects[0].Size,
        LastModified = s3Objects[0].LastModified,
        ContentType = response.ContentType
      };

      return View(modelObject);
    }

    //
    // GET: /S3Object/Create

    public ActionResult Create(string prefix = "", int maxKeys = 100)
    {
      ViewBag.prefix = prefix;
      ViewBag.maxKeys = maxKeys;

      return View();
    }

    //
    // POST: /S3Object/Create

    [HttpPost]
    public ActionResult Create(S3Object s3object, string prefix = "", int maxKeys = 100)
    {
      ViewBag.prefix = prefix;
      ViewBag.maxKeys = maxKeys;

      if (ModelState.IsValid && Request.Files.Count > 0)
      {
        var put = new Amazon.S3.Model.PutObjectRequest();
        put.WithBucketName(WebConfigurationManager.AppSettings["UploadBucket"]);
        put.WithKey(s3object.Key);

        var file = Request.Files[0];
        put.WithContentType(file.ContentType);
        put.WithInputStream(file.InputStream);
        put.WithCannedACL(Amazon.S3.Model.S3CannedACL.PublicRead);
        Amazon.S3.Model.PutObjectResponse result = s3.PutObject(put);

        return RedirectToAction("Index", new { prefix = prefix, maxKeys = maxKeys });
      }

      return View(s3object);
    }

    //
    // GET: /S3Object/BulkCreate
    public ActionResult BulkCreate(string prefix = "", int maxKeys = 100)
    {
      ViewBag.prefix = prefix;
      ViewBag.maxKeys = maxKeys;

      return View();
    }

    //
    // POST: /S3Object/BulkCreate

    [HttpPost, ActionName("BulkCreate")]
    public ActionResult BulkCreateConfirmed(string prefix = "", int maxKeys = 100)
    {
      ViewBag.prefix = prefix;
      ViewBag.maxKeys = maxKeys;

      byte[] buffer = new byte[4096];

      if (Request.Files.Count > 0)
      {
        try
        {
          using (var fileStream = Request.Files[0].InputStream)
          {
            ZipFile zf = new ZipFile(fileStream);

            foreach (ZipEntry zipEntry in zf)
            {
              if (!zipEntry.IsFile)
              {
                continue;
              }

              string entryFileName = zipEntry.Name;
              string entryKey = prefix + entryFileName;

              using (MemoryStream ms = new MemoryStream((int)zipEntry.Size))
              {
                ICSharpCode.SharpZipLib.Core.StreamUtils.Copy(zf.GetInputStream(zipEntry), ms, buffer);

                ms.Seek(0, SeekOrigin.Begin);

                var put = new Amazon.S3.Model.PutObjectRequest();
                put.WithBucketName(WebConfigurationManager.AppSettings["UploadBucket"]);
                put.WithCannedACL(Amazon.S3.Model.S3CannedACL.PublicRead);

                put.WithKey(entryKey);
                put.WithInputStream(ms);
                Amazon.S3.Model.PutObjectResponse result = s3.PutObject(put);

              }


            }
          }

          return RedirectToAction("Index", new { prefix = prefix, maxKeys = maxKeys });
        }
        catch (Exception ex)
        {
          Request.RequestContext.HttpContext.Trace.Write(ex.Message);
        }
      }

      return View();
    }

    //
    // GET: /S3Object/Edit/5

    public ActionResult Edit(string id = null, string prefix = "", int maxKeys = 100)
    {
      ViewBag.prefix = prefix;
      ViewBag.maxKeys = maxKeys;

      var list = new Amazon.S3.Model.ListObjectsRequest();
      list.WithBucketName(WebConfigurationManager.AppSettings["UploadBucket"]);
      list.WithPrefix(id);

      var s3Objects = s3.ListObjects(list).S3Objects;
      if (s3Objects.Count == 0)
      {
        return HttpNotFound();
      }

      var get = new Amazon.S3.Model.GetObjectRequest();
      get.WithBucketName(WebConfigurationManager.AppSettings["UploadBucket"]);
      get.WithKey(s3Objects[0].Key);

      var response = s3.GetObject(get);

      S3Object modelObject = new S3Object
      {
        Key = s3Objects[0].Key,
        Size = s3Objects[0].Size,
        LastModified = s3Objects[0].LastModified,
        ContentType = response.ContentType
      };

      return View(modelObject);
    }

    //
    // POST: /S3Object/Edit/5

    [HttpPost]
    public ActionResult Edit(S3Object s3object, string prefix = "", int maxKeys = 100)
    {
      ViewBag.prefix = prefix;
      ViewBag.maxKeys = maxKeys;

      if (ModelState.IsValid && Request.Files.Count > 0)
      {
        var put = new Amazon.S3.Model.PutObjectRequest();
        put.WithBucketName(WebConfigurationManager.AppSettings["UploadBucket"]);
        put.WithKey(s3object.Key);

        var file = Request.Files[0];
        put.WithContentType(file.ContentType);
        put.WithInputStream(file.InputStream);
        put.WithCannedACL(Amazon.S3.Model.S3CannedACL.PublicRead);
        s3.PutObject(put);

        return RedirectToAction("Index", new { prefix = prefix, maxKeys = maxKeys });
      }

      return View(s3object);
    }

    //
    // GET: /S3Object/Delete/5

    public ActionResult Delete(string id = null, string prefix = "", int maxKeys = 100)
    {
      ViewBag.prefix = prefix;
      ViewBag.maxKeys = maxKeys;

      var list = new Amazon.S3.Model.ListObjectsRequest();
      list.WithBucketName(WebConfigurationManager.AppSettings["UploadBucket"]);
      list.WithPrefix(id);

      var s3Objects = s3.ListObjects(list).S3Objects;
      if (s3Objects.Count == 0)
      {
        return HttpNotFound();
      }


      var get = new Amazon.S3.Model.GetObjectRequest();
      get.WithBucketName(WebConfigurationManager.AppSettings["UploadBucket"]);
      get.WithKey(s3Objects[0].Key);

      var response = s3.GetObject(get);

      S3Object modelObject = new S3Object
      {
        Key = s3Objects[0].Key,
        Size = s3Objects[0].Size,
        LastModified = s3Objects[0].LastModified,
        ContentType = response.ContentType
      };

      return View(modelObject);
    }

    //
    // POST: /S3Object/Delete/5

    [HttpPost, ActionName("Delete")]
    public ActionResult DeleteConfirmed(string id, string prefix = "", int maxKeys = 100)
    {
      ViewBag.prefix = prefix;
      ViewBag.maxKeys = maxKeys;

      var delete = new Amazon.S3.Model.DeleteObjectRequest();
      delete.WithBucketName(WebConfigurationManager.AppSettings["UploadBucket"]);
      delete.WithKey(id);
      s3.DeleteObject(delete);
      return RedirectToAction("Index", new { prefix = prefix, maxKeys = maxKeys });
    }

    //
    // GET: /S3Object/BulkDelete

    public ActionResult BulkDelete(string prefix = "", int maxKeys = 100)
    {
      ViewBag.prefix = prefix;
      ViewBag.maxKeys = maxKeys;

      var list = new Amazon.S3.Model.ListObjectsRequest();
      list.WithBucketName(WebConfigurationManager.AppSettings["UploadBucket"]);
      if (!String.IsNullOrWhiteSpace(prefix))
      {
        list.WithPrefix(prefix);
      }

      list.WithMaxKeys(maxKeys);

      var s3Objects = s3.ListObjects(list);
      List<S3Object> modelObjects = new List<S3Object>(s3Objects.S3Objects.Count);

      foreach (var s3Object in s3Objects.S3Objects)
      {
        modelObjects.Add(new S3Object
        {
          Key = s3Object.Key,
          Size = s3Object.Size,
          LastModified = s3Object.LastModified
        });
      }

      return View(modelObjects);
    }

    [HttpPost, ActionName("BulkDelete")]
    public ActionResult BulkDeleteConfirmed(string prefix = "", int maxKeys = 100)
    {
      if (!String.IsNullOrWhiteSpace(prefix))
      {
        ViewBag.prefix = prefix;
        ViewBag.maxKeys = maxKeys;

        var list = new Amazon.S3.Model.ListObjectsRequest();
        list.WithBucketName(WebConfigurationManager.AppSettings["UploadBucket"]);
        if (!String.IsNullOrWhiteSpace(prefix))
        {
          list.WithPrefix(prefix);
        }

        list.WithMaxKeys(maxKeys);

        var keys = s3.ListObjects(list).S3Objects.Select(e => new Amazon.S3.Model.KeyVersion(e.Key)).ToArray();

        var bulkDelete = new Amazon.S3.Model.DeleteObjectsRequest();
        bulkDelete.WithBucketName(WebConfigurationManager.AppSettings["UploadBucket"]);
        bulkDelete.WithKeys(keys);
        s3.DeleteObjects(bulkDelete);
      }

      return RedirectToAction("Index", new { prefix = prefix, maxKeys = maxKeys });
    }

    protected override void Dispose(bool disposing)
    {
      db.Dispose();
      s3.Dispose();
      base.Dispose(disposing);
    }
  }
}