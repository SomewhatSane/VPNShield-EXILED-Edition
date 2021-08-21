<?php
    require('config.php');
    header('Content-Type: application/json');

    if (!isset($_GET['Branch'])) {
        $jsonData = [
            "StatusCode" => -1,
            "Message" => "No branch specified."
        ];
        http_response_code(400);
        die(json_encode($jsonData));
    }

    if (!isset($_GET['CurrentVersion'])) {
        $jsonData = [
            "StatusCode" => -2,
            "Message" => "No current version specified."
        ];
        http_response_code(400);
        die(json_encode($jsonData));
    }

    $branch = $_GET['Branch'];
    $currentVersion = $_GET['CurrentVersion'];

    switch ($branch) {
        case "Stable":
            $latestVersion = $latestStable;
            $downloadUrls = $downloadUrlsStable;
            break;
        case "Beta":
            $latestVersion = $latestBeta;
            $downloadUrls = $downloadUrlsBeta;
            break;
        default:
            $jsonData = [
                "StatusCode" => -3,
                "Message" => "Invalid branch specified ({$branch})."
            ];
            http_response_code(400);
            die(json_encode($jsonData));
    }

    if ($currentVersion == $latestVersion) {
        $jsonData = [
            "StatusCode" => 0,
            "Branch" => "{$branch}",
            "Message" => "No updates available. You are running the latest version of VPNShield."
        ];
        die(json_encode($jsonData));
    }

    else {
        $jsonData = [
            "StatusCode" => 1,
            "Branch" => "{$branch}",
            "Message" => "An update is available!",
            "LatestVersion" => "{$latestVersion}",
            "DownloadUrls" => $downloadUrls
        ];
        die(json_encode($jsonData));
    }
?>