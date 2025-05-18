#!/bin/bash
set -Eeuo pipefail

DIR_MODS_SYS="/opt/dst_server/mods"
DIR_MODS_USER="${DST_USER_DATA_PATH}/mods"
FILE_CLUSTER_TOKEN="${DST_USER_DATA_PATH}/cluster_token.txt"

# set -e error handler.
on_error() {
    echo >&2 "Error on line ${1}${3+: ${3}}; RET ${2}."
    exit "$2"
}
trap 'on_error ${LINENO} $?' ERR 2>/dev/null || true # some shells don't have ERR trap.

# fix config file permission
#chown -R "${DST_USER}:${DST_GROUP}" "${DST_USER_DATA_PATH}"

# protect our mods dir
# if the mods dir is already a symlink, then we temporary remove it to protect it, so that it survives a container restart
if [[ -L "${DIR_MODS_SYS}" ]]; then
	rm -f "${DIR_MODS_SYS}"
    cp -r /opt/dst_default_config/mods "${DIR_MODS_SYS}"
fi
# Update game
# note that the update process modifies (resets) the mods folder so we symlink that later
echo "Updating server..."
sleep 2
steamcmd +runscript /opt/steamcmd_scripts/install_dst_server


# if there are no mods config, use the one that comes with the server
if [ ! -d "${DIR_MODS_USER}" ]; then
    echo "Creating default mod config..."
    mkdir -p "${DST_USER_DATA_PATH}"
    cp -r "${DIR_MODS_SYS}" "${DIR_MODS_USER}"
fi

# override server mods folder with the user provided one
rm -rf "${DIR_MODS_SYS}"
ln -s "${DIR_MODS_USER}" "${DIR_MODS_SYS}"

# update mods
# Note: cluster-agnostic downloading is somehow broken
# https://forums.kleientertainment.com/forums/topic/128188-what-is-ugc/?do=findComment&comment=1440420
echo "Updating mods..."
su --preserve-environment --group "${DST_GROUP}" -c "dontstarve_dedicated_server_nullrenderer -persistent_storage_root \"${DST_USER_DATA_PATH}\" -ugc_directory \"${DST_USER_DATA_PATH}\"/ugc -cluster DST -only_update_server_mods" "${DST_USER}"

# start tmux sesion to interact with servers' stdins
DST_USER_DATA_PATH="${DST_USER_DATA_PATH:-/data}"
SESSION_NAME="server"
MASTER_LOG=/tmp/dst-master.log
CAVES_LOG=/tmp/dst-caves.log

tmux new-session -d -s "$SESSION_NAME" -n master \
"dontstarve_dedicated_server_nullrenderer -skip_update_server_mods -persistent_storage_root \
    $DST_USER_DATA_PATH -ugc_directory $DST_USER_DATA_PATH/ugc -cluster DST -shard Master \
    2>&1 | tee $MASTER_LOG; \
    tmux wait-for -S master-done; \
    tmux send-keys -t server:1 \"c_shutdown()\" Enter"

tmux new-window -t "$SESSION_NAME" -n caves \
    "dontstarve_dedicated_server_nullrenderer -skip_update_server_mods -persistent_storage_root \
    $DST_USER_DATA_PATH -ugc_directory $DST_USER_DATA_PATH/ugc -cluster DST -shard Caves \
    2>&1 | tee $CAVES_LOG; \
    tmux wait-for -S caves-done"

while [[ ! -f "$MASTER_LOG" || ! -f "$CAVES_LOG" ]]; do sleep 1; done

# Start tailing logs in the background
tail -n 0 -f "$MASTER_LOG" "$CAVES_LOG" &
TAIL_PID=$!

# Wait for the events to finish
tmux wait-for master-done
tmux wait-for caves-done

# Kill the tail process after completion
kill $TAIL_PID

echo "Both master and caves have completed."
exit 0

