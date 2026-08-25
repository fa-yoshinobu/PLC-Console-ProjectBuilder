# TODO

PLC Console ProjectBuilder の残りの公開作業と保守作業を管理する。

## 公開後 / 公開前の確認

- [x] Windows 版リリースにコード署名を付けるか決める。
  正式な Windows コード署名は有料証明書が必要になるため採用しない。
  GitHub Release パッケージは、インストーラーなし / コード署名なしの自己完結型 single-file EXE として公開する。
- [ ] 各リリース後に GitHub Release の配布ファイルを確認する。
  `PLCConsoleProjectBuilder-win-x64.zip` に `PLCConsoleProjectBuilder.exe` が含まれ、クリーンな Windows PC で起動することを確認する。
- [ ] 各リリース前に公開マニュアルへのリンクを確認する。
  アプリの Help メニューは `https://plc-console.fa-labo.com/` を開き、上部ヘッダーのリンクは `https://plc-console.fa-labo.com/projectbuilder/projectbuilder.html` を開く。
- [x] ZIP 配布のみを継続し、インストーラーは追加しない。
  GitHub Releases では、自己完結型 single-file の `PLCConsoleProjectBuilder.exe` だけを含む ZIP を配布する。

## 保守メモ

- [x] ビルド出力は Git 管理外にしている。
  `dotnet/publish/` と `artifacts/` は意図的に Git から除外する。
- [x] ProjectBuilder マニュアルへのリンクはアプリ UI に配置済み。
  Help メニューは全体マニュアルサイトを開き、ヘッダーリンクは ProjectBuilder マニュアルページを開く。
- [x] リリースビルドの出力先は文書化済み。
  `build-dotnet-onefile.bat` で `dotnet/publish/win-x64/PLCConsoleProjectBuilder.exe` を生成する。
