mergeInto(LibraryManager.library, {
  SyncFs: function() {
    FS.syncfs(false, function (err) {
      if (err) console.error(err);
    });
  }
});