# Slay the Spire 2 新キャラMOD

新キャラの企画、制作、Steam版での動作確認、配布をこのリポジトリで管理する。初版の進捗は [Issue #1](https://github.com/KAFKA2306/sts2/issues/1) に記録する。

## セットアップ

このリポジトリを取得したら、リポジトリのルートで `git submodule update --init` を実行する。[kociden/sakurai-skill](https://github.com/kociden/sakurai-skill) が `.agents/skills/sakurai-skill` に取得され、Codexはその中の `skills/sakurai/SKILL.md` を読み込む。

## 制作と確認

1. キャラの見た目と、戦闘でプレイヤーに何を選ばせたいかを決める。設計レビューでは、Codexのプロジェクトスキル [sakurai](https://github.com/kociden/sakurai-skill) を使う。
2. [STS2 Character Mod Creator](https://slay.spencerstiles.com/) で初期デッキ、固有カード、初期レリック、立ち絵を作る。再編集用JSONを `project/character.json`、共有できる素材原本を `assets/` に置き、関連Issueを添えてPRでmainに取り込む。
3. 本体のバージョンに合わせてZIPを書き出す。WindowsのSteam版で、自作MODと対応するBaseLibを有効にする。キャラ選択、初期カードとレリック、固有効果、戦闘後の報酬を確認し、本体とBaseLibの版、設計コミット、結果をIssueに残す。
4. 確認したコミットにタグを付け、動作確認済みZIPと導入手順をGitHub Releaseに載せる。Steam Workshopでも配布する場合は、対応するWorkshop URLをReleaseに追記する。

## 参照

- [Creatorの書き出しとインストール](https://slay.spencerstiles.com/guide#export--install)
- [GitHub Releases](https://docs.github.com/en/repositories/releasing-projects-on-github/managing-releases-in-a-repository)
- [Steam Workshop公式アップローダー](https://github.com/megacrit/sts2-mod-uploader)
