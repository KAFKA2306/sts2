# STS2 MODの管理ルール

## 正本

- `main` を唯一の常設ブランチにする。実装、カードとレリックのデータ、再ビルド用スクリプト、評価ケースをここに置く。
- [Issue #1](https://github.com/KAFKA2306/sts2/issues/1) で初版の遊びと合格条件を管理する。仕掛けや数値を変えるPRでは、関連Issueをリンクし、受け入れ条件も更新する。
- READMEはプロジェクトの入口にする。配布版の対応環境と導入手順は該当する [GitHub Release](https://github.com/KAFKA2306/sts2/releases) に載せる。

## 変更から統合まで

1. 関連Issueを読み、変更範囲と実際に確かめることを記録する。新しい作業は `main` から短命のブランチを切り、1つの目的を1つのPRで扱う。
2. 実装し、変更に見合う確認を行う。文書なら差分とリンク、ゲームに効く変更ならビルドと該当するSteam盤面を確認する。PRに確認した内容と結果を記録する。
3. 必須チェックの結果を確認して `main` へマージし、反映を読み戻す。作業ブランチの最終コミットをタグに残し、リモートでタグを確認してからブランチを削除する。配布用のタグはReleaseに対応させる。
4. Issueの合格条件を満たしたら閉じる。環境や権限が揃わず自律作業を完了できない場合は、実施結果と再開条件をIssueに記録して閉じる。

## 制作と配布

- 制作はAlchyrの新キャラテンプレート、BaseLib、MegaDotの経路に統一する。直接追加する外部機能は2026年の公開履歴を確認し、実際に使った版またはコミットを固定する。桜井スキルは既存の上流サブモジュールを使う。
- スクリプトがビルド、固定行動の再生、期待値との照合、証跡保存、配布を担当する。設計と修正を担当するエージェントは一体。再生と再採点は保存済みの入力と期待値を使う。設計時は関係する仕様と失敗ケースの差分を渡す。
- Steam実測では本体のブランチとBuildID、BaseLibの版、ソースのコミット、ZIPのSHA-256、ケースの結果を紐づける。実測に使った同じZIPをReleaseに添付し、対応版と導入手順を記載する。リポジトリには配布権のある素材と再現に必要な入力を置く。ゲーム本体、個人のセーブ、資格情報は各実行環境に置く。

[GitHubのPR統合手順](https://docs.github.com/en/pull-requests/how-tos/merge-and-close-pull-requests/merging-a-pull-request) · [GitHub Releasesの手順](https://docs.github.com/en/repositories/releasing-projects-on-github/managing-releases-in-a-repository) · [Alchyrのセットアップ](https://github.com/Alchyr/ModTemplate-StS2/wiki/Setup)
