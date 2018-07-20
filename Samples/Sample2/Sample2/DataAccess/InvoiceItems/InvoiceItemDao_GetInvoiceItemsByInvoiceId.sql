SELECT InvoiceLineId
     , invoice_items.InvoiceId
     , invoice_items.UnitPrice
     , Quantity
     , invoice_items.TrackId
     , tracks.Name AS TrackName
     , albums.Title AS AlbumTitle
     , artists.Name AS ArtistName
  FROM invoice_items
     , tracks
     , albums
     , artists 
 WHERE invoice_items.TrackId = tracks.TrackId 
   AND tracks.AlbumId = albums.AlbumId 
   AND albums.ArtistId = artists.ArtistId 
   AND InvoiceId = @InvoiceId