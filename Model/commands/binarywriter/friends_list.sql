COPY friends_list (
player_id,
accepting_friend_invites,
friends,
blocked,
sent_friend_invites,
received_friend_invites,
list_version
) FROM STDIN (FORMAT BINARY);