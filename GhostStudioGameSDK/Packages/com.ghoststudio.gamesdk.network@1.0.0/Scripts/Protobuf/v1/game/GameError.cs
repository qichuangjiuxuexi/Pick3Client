// <auto-generated>
//     Generated by the protocol buffer compiler.  DO NOT EDIT!
//     source: game/game_error.proto
// </auto-generated>
#pragma warning disable 1591, 0612, 3021, 8981
#region Designer generated code

using pb = global::Google.Protobuf;
using pbc = global::Google.Protobuf.Collections;
using pbr = global::Google.Protobuf.Reflection;
using scg = global::System.Collections.Generic;
namespace API.V1.Game {

  /// <summary>Holder for reflection information generated from game/game_error.proto</summary>
  public static partial class GameErrorReflection {

    #region Descriptor
    /// <summary>File descriptor for game/game_error.proto</summary>
    public static pbr::FileDescriptor Descriptor {
      get { return descriptor; }
    }
    private static pbr::FileDescriptor descriptor;

    static GameErrorReflection() {
      byte[] descriptorData = global::System.Convert.FromBase64String(
          string.Concat(
            "ChVnYW1lL2dhbWVfZXJyb3IucHJvdG8SDXYxLmdhbWUuZXJyb3IinQEKDUVy",
            "cm9yUmVzcG9uc2USDAoEY29kZRgBIAEoBRIPCgdtZXNzYWdlGAIgASgJEjwK",
            "CG1ldGFkYXRhGAMgAygLMioudjEuZ2FtZS5lcnJvci5FcnJvclJlc3BvbnNl",
            "Lk1ldGFkYXRhRW50cnkaLwoNTWV0YWRhdGFFbnRyeRILCgNrZXkYASABKAkS",
            "DQoFdmFsdWUYAiABKAk6AjgBKuwHCgtFcnJvclJlYXNvbhILCgdTVUNDRVNT",
            "EAASCgoGRkFJTEVEEAESDwoLSU5WQUxJRF9VUkwQZRIXChNBVVRIRU5USUNB",
            "VEVfRkFJTEVEEGYSFAoQUkVBRF9CT0RZX0ZBSUxFRBBnEhIKDkRFQ1JZUFRf",
            "RkFJTEVEEGgSEQoNREVDT0RFX0ZBSUxFRBBpEhUKEUlOVkFMSURfUExBWUVS",
            "X0lEEGoSFQoRSU5WQUxJRF9QQVJBTUVURVIQaxIUChBBVVRIT1JJWkVfRkFJ",
            "TEVEEGwSEQoNRU5DT0RFX0ZBSUxFRBBtEhIKDkVOQ1JZUFRfRkFJTEVEEG4S",
            "FQoRV1JJVEVfQk9EWV9GQUlMRUQQbxIUChBQTEFZRVJfTk9UX0ZPVU5EEHAS",
            "FgoSSU5WQUxJRF9GRUFUVVJFX0lEEHESDwoLUEFSQU1fRVJST1IQchIbChdD",
            "UkVBVEVfQ0xVQl9OQU1FX1JFUEVBVBBzEhMKD0lOVkFMSURfQ0xVQl9JRBB0",
            "EhUKEUpPSU5fTEVWRUxfSVNfTE9XEHUSHQoZSk9JTl9MRVZFTF9NRU1CRVJf",
            "SVNfRlVMTBB2EhsKF0VYSVRfQ0xVQl9NRU1CRVJfTk9UX0lOEHcSJQohVVBE",
            "QVRFX01FTUJFUl9CQVNJQ19NRU1CRVJfTk9UX0lOEHgSJQohVVBEQVRFX01F",
            "TUJFUl9TQ09SRV9NRU1CRVJfTk9UX0lOEHkSIwofVVBEQVRFX0NMVUJfQkFT",
            "SUNfTUVNQkVSX05PVF9JThB6EicKI1VQREFURV9DTFVCX0JBU0lDX01FTUJF",
            "Ul9OT1RfTEVBREVSEHsSHQoZS0lDS19NRU1CRVJfUExBWUVSX05PVF9JThB8",
            "EiEKHUtJQ0tfTUVNQkVSX09QVF9QTEFZRVJfTk9UX0lOEH0SGQoVS0lDS19N",
            "RU1CRVJfTk9UX0FETUlOEH4SFAoQS0lDS19NRU1CRVJfU0VMRhB/EhwKF1VQ",
            "REFURV9NRU1CRVJfUk9MRV9TRUxGEIABEh4KGVVQREFURV9NRU1CRVJfUk9M",
            "RV9OT1RfSU4QgQESIgodVVBEQVRFX01FTUJFUl9ST0xFX05PVF9MRUFERVIQ",
            "ggESFwoSUExBWUVSX05PVF9JTl9DTFVCEIMBEhIKDVNUT1JBR0VfRVJST1IQ",
            "yQESJgogUExBWUVSX1VQTE9BRF9JTlZBTElEX0NMSUVOVF9WRVIQzY8GEiYK",
            "IFBMQVlFUl9VUExPQURfSU5WQUxJRF9TRVJWRVJfVkVSEM6PBhIpCiNQTEFZ",
            "RVJfR0VUX0xPQ0FUSU9OX1BBUlNFX0lQX0ZBSUxFRBD5kQZCIFoQYXBpL3Yx",
            "L2dhbWU7Z2FtZaoCC0FQSS5WMS5HYW1lYgZwcm90bzM="));
      descriptor = pbr::FileDescriptor.FromGeneratedCode(descriptorData,
          new pbr::FileDescriptor[] { },
          new pbr::GeneratedClrTypeInfo(new[] {typeof(global::API.V1.Game.ErrorReason), }, null, new pbr::GeneratedClrTypeInfo[] {
            new pbr::GeneratedClrTypeInfo(typeof(global::API.V1.Game.ErrorResponse), global::API.V1.Game.ErrorResponse.Parser, new[]{ "Code", "Message", "Metadata" }, null, null, null, new pbr::GeneratedClrTypeInfo[] { null, })
          }));
    }
    #endregion

  }
  #region Enums
  public enum ErrorReason {
    /// <summary>
    /// 成功
    /// </summary>
    [pbr::OriginalName("SUCCESS")] Success = 0,
    /// <summary>
    /// 通用错误码
    /// </summary>
    [pbr::OriginalName("FAILED")] Failed = 1,
    /// <summary>
    /// 客户端错误
    /// </summary>
    [pbr::OriginalName("INVALID_URL")] InvalidUrl = 101,
    /// <summary>
    /// 认证失败：验证jwt token无效
    /// </summary>
    [pbr::OriginalName("AUTHENTICATE_FAILED")] AuthenticateFailed = 102,
    /// <summary>
    /// 读取request body失败
    /// </summary>
    [pbr::OriginalName("READ_BODY_FAILED")] ReadBodyFailed = 103,
    /// <summary>
    /// 解密失败
    /// </summary>
    [pbr::OriginalName("DECRYPT_FAILED")] DecryptFailed = 104,
    /// <summary>
    /// 解码失败
    /// </summary>
    [pbr::OriginalName("DECODE_FAILED")] DecodeFailed = 105,
    /// <summary>
    /// 不合法的playerId：token和消息体中的playerId不一致
    /// </summary>
    [pbr::OriginalName("INVALID_PLAYER_ID")] InvalidPlayerId = 106,
    /// <summary>
    /// 参数自动校验失败
    /// </summary>
    [pbr::OriginalName("INVALID_PARAMETER")] InvalidParameter = 107,
    /// <summary>
    /// 授权失败：生成jwt token失败
    /// </summary>
    [pbr::OriginalName("AUTHORIZE_FAILED")] AuthorizeFailed = 108,
    /// <summary>
    /// 编码失败
    /// </summary>
    [pbr::OriginalName("ENCODE_FAILED")] EncodeFailed = 109,
    /// <summary>
    /// 加密失败
    /// </summary>
    [pbr::OriginalName("ENCRYPT_FAILED")] EncryptFailed = 110,
    /// <summary>
    /// 写入response body失败
    /// </summary>
    [pbr::OriginalName("WRITE_BODY_FAILED")] WriteBodyFailed = 111,
    /// <summary>
    /// 根据playerId未在数据库中查找到玩家数据
    /// </summary>
    [pbr::OriginalName("PLAYER_NOT_FOUND")] PlayerNotFound = 112,
    /// <summary>
    /// featureId无效：排行榜没有此配置
    /// </summary>
    [pbr::OriginalName("INVALID_FEATURE_ID")] InvalidFeatureId = 113,
    /// <summary>
    ///工会:
    /// </summary>
    [pbr::OriginalName("PARAM_ERROR")] ParamError = 114,
    /// <summary>
    ///创建工会名称重复
    /// </summary>
    [pbr::OriginalName("CREATE_CLUB_NAME_REPEAT")] CreateClubNameRepeat = 115,
    /// <summary>
    ///无效工会ID
    /// </summary>
    [pbr::OriginalName("INVALID_CLUB_ID")] InvalidClubId = 116,
    /// <summary>
    ///加入工会玩家等级低于工会限制等级
    /// </summary>
    [pbr::OriginalName("JOIN_LEVEL_IS_LOW")] JoinLevelIsLow = 117,
    /// <summary>
    ///加入工会,成员已满
    /// </summary>
    [pbr::OriginalName("JOIN_LEVEL_MEMBER_IS_FULL")] JoinLevelMemberIsFull = 118,
    /// <summary>
    ///退出工会,玩家不在工会中
    /// </summary>
    [pbr::OriginalName("EXIT_CLUB_MEMBER_NOT_IN")] ExitClubMemberNotIn = 119,
    /// <summary>
    ///更新成员信息,玩家不在工会中
    /// </summary>
    [pbr::OriginalName("UPDATE_MEMBER_BASIC_MEMBER_NOT_IN")] UpdateMemberBasicMemberNotIn = 120,
    /// <summary>
    ///更新成员分数,玩家不在工会中
    /// </summary>
    [pbr::OriginalName("UPDATE_MEMBER_SCORE_MEMBER_NOT_IN")] UpdateMemberScoreMemberNotIn = 121,
    /// <summary>
    ///更新工会基本信息,玩家不在工会中
    /// </summary>
    [pbr::OriginalName("UPDATE_CLUB_BASIC_MEMBER_NOT_IN")] UpdateClubBasicMemberNotIn = 122,
    /// <summary>
    ///更新工会基本信息,玩家不是Leader
    /// </summary>
    [pbr::OriginalName("UPDATE_CLUB_BASIC_MEMBER_NOT_LEADER")] UpdateClubBasicMemberNotLeader = 123,
    /// <summary>
    ///工会踢人,玩家不在工会中
    /// </summary>
    [pbr::OriginalName("KICK_MEMBER_PLAYER_NOT_IN")] KickMemberPlayerNotIn = 124,
    /// <summary>
    ///工会踢人,被操作玩家不在工会中
    /// </summary>
    [pbr::OriginalName("KICK_MEMBER_OPT_PLAYER_NOT_IN")] KickMemberOptPlayerNotIn = 125,
    /// <summary>
    ///工会踢人,玩家不是管理员
    /// </summary>
    [pbr::OriginalName("KICK_MEMBER_NOT_ADMIN")] KickMemberNotAdmin = 126,
    /// <summary>
    ///工会踢人,自己踢自己
    /// </summary>
    [pbr::OriginalName("KICK_MEMBER_SELF")] KickMemberSelf = 127,
    /// <summary>
    ///修改成员角色,自己改自己
    /// </summary>
    [pbr::OriginalName("UPDATE_MEMBER_ROLE_SELF")] UpdateMemberRoleSelf = 128,
    /// <summary>
    ///修改成员角色,不在工会中
    /// </summary>
    [pbr::OriginalName("UPDATE_MEMBER_ROLE_NOT_IN")] UpdateMemberRoleNotIn = 129,
    /// <summary>
    ///修改成员角色,不是Leader
    /// </summary>
    [pbr::OriginalName("UPDATE_MEMBER_ROLE_NOT_LEADER")] UpdateMemberRoleNotLeader = 130,
    /// <summary>
    ///玩家不在工会中
    /// </summary>
    [pbr::OriginalName("PLAYER_NOT_IN_CLUB")] PlayerNotInClub = 131,
    /// <summary>
    /// 服务器错误
    /// </summary>
    [pbr::OriginalName("STORAGE_ERROR")] StorageError = 201,
    /// <summary>
    /// player/download: 1002XX
    /// player/upload: 1003XX
    /// </summary>
    [pbr::OriginalName("PLAYER_UPLOAD_INVALID_CLIENT_VER")] PlayerUploadInvalidClientVer = 100301,
    /// <summary>
    /// 服务器存储的玩家数据中的服务器版本更高
    /// </summary>
    [pbr::OriginalName("PLAYER_UPLOAD_INVALID_SERVER_VER")] PlayerUploadInvalidServerVer = 100302,
    /// <summary>
    /// player/delete: 1004XX
    /// player/haertbeat: 1005XX
    /// player/get-location: 1006XX
    /// </summary>
    [pbr::OriginalName("PLAYER_GET_LOCATION_PARSE_IP_FAILED")] PlayerGetLocationParseIpFailed = 100601,
  }

  #endregion

  #region Messages
  [global::System.Diagnostics.DebuggerDisplayAttribute("{ToString(),nq}")]
  public sealed partial class ErrorResponse : pb::IMessage<ErrorResponse>
  #if !GOOGLE_PROTOBUF_REFSTRUCT_COMPATIBILITY_MODE
      , pb::IBufferMessage
  #endif
  {
    private static readonly pb::MessageParser<ErrorResponse> _parser = new pb::MessageParser<ErrorResponse>(() => new ErrorResponse());
    private pb::UnknownFieldSet _unknownFields;
    [global::System.Diagnostics.DebuggerNonUserCodeAttribute]
    [global::System.CodeDom.Compiler.GeneratedCode("protoc", null)]
    public static pb::MessageParser<ErrorResponse> Parser { get { return _parser; } }

    [global::System.Diagnostics.DebuggerNonUserCodeAttribute]
    [global::System.CodeDom.Compiler.GeneratedCode("protoc", null)]
    public static pbr::MessageDescriptor Descriptor {
      get { return global::API.V1.Game.GameErrorReflection.Descriptor.MessageTypes[0]; }
    }

    [global::System.Diagnostics.DebuggerNonUserCodeAttribute]
    [global::System.CodeDom.Compiler.GeneratedCode("protoc", null)]
    pbr::MessageDescriptor pb::IMessage.Descriptor {
      get { return Descriptor; }
    }

    [global::System.Diagnostics.DebuggerNonUserCodeAttribute]
    [global::System.CodeDom.Compiler.GeneratedCode("protoc", null)]
    public ErrorResponse() {
      OnConstruction();
    }

    partial void OnConstruction();

    [global::System.Diagnostics.DebuggerNonUserCodeAttribute]
    [global::System.CodeDom.Compiler.GeneratedCode("protoc", null)]
    public ErrorResponse(ErrorResponse other) : this() {
      code_ = other.code_;
      message_ = other.message_;
      metadata_ = other.metadata_.Clone();
      _unknownFields = pb::UnknownFieldSet.Clone(other._unknownFields);
    }

    [global::System.Diagnostics.DebuggerNonUserCodeAttribute]
    [global::System.CodeDom.Compiler.GeneratedCode("protoc", null)]
    public ErrorResponse Clone() {
      return new ErrorResponse(this);
    }

    /// <summary>Field number for the "code" field.</summary>
    public const int CodeFieldNumber = 1;
    private int code_;
    [global::System.Diagnostics.DebuggerNonUserCodeAttribute]
    [global::System.CodeDom.Compiler.GeneratedCode("protoc", null)]
    public int Code {
      get { return code_; }
      set {
        code_ = value;
      }
    }

    /// <summary>Field number for the "message" field.</summary>
    public const int MessageFieldNumber = 2;
    private string message_ = "";
    [global::System.Diagnostics.DebuggerNonUserCodeAttribute]
    [global::System.CodeDom.Compiler.GeneratedCode("protoc", null)]
    public string Message {
      get { return message_; }
      set {
        message_ = pb::ProtoPreconditions.CheckNotNull(value, "value");
      }
    }

    /// <summary>Field number for the "metadata" field.</summary>
    public const int MetadataFieldNumber = 3;
    private static readonly pbc::MapField<string, string>.Codec _map_metadata_codec
        = new pbc::MapField<string, string>.Codec(pb::FieldCodec.ForString(10, ""), pb::FieldCodec.ForString(18, ""), 26);
    private readonly pbc::MapField<string, string> metadata_ = new pbc::MapField<string, string>();
    [global::System.Diagnostics.DebuggerNonUserCodeAttribute]
    [global::System.CodeDom.Compiler.GeneratedCode("protoc", null)]
    public pbc::MapField<string, string> Metadata {
      get { return metadata_; }
    }

    [global::System.Diagnostics.DebuggerNonUserCodeAttribute]
    [global::System.CodeDom.Compiler.GeneratedCode("protoc", null)]
    public override bool Equals(object other) {
      return Equals(other as ErrorResponse);
    }

    [global::System.Diagnostics.DebuggerNonUserCodeAttribute]
    [global::System.CodeDom.Compiler.GeneratedCode("protoc", null)]
    public bool Equals(ErrorResponse other) {
      if (ReferenceEquals(other, null)) {
        return false;
      }
      if (ReferenceEquals(other, this)) {
        return true;
      }
      if (Code != other.Code) return false;
      if (Message != other.Message) return false;
      if (!Metadata.Equals(other.Metadata)) return false;
      return Equals(_unknownFields, other._unknownFields);
    }

    [global::System.Diagnostics.DebuggerNonUserCodeAttribute]
    [global::System.CodeDom.Compiler.GeneratedCode("protoc", null)]
    public override int GetHashCode() {
      int hash = 1;
      if (Code != 0) hash ^= Code.GetHashCode();
      if (Message.Length != 0) hash ^= Message.GetHashCode();
      hash ^= Metadata.GetHashCode();
      if (_unknownFields != null) {
        hash ^= _unknownFields.GetHashCode();
      }
      return hash;
    }

    [global::System.Diagnostics.DebuggerNonUserCodeAttribute]
    [global::System.CodeDom.Compiler.GeneratedCode("protoc", null)]
    public override string ToString() {
      return pb::JsonFormatter.ToDiagnosticString(this);
    }

    [global::System.Diagnostics.DebuggerNonUserCodeAttribute]
    [global::System.CodeDom.Compiler.GeneratedCode("protoc", null)]
    public void WriteTo(pb::CodedOutputStream output) {
    #if !GOOGLE_PROTOBUF_REFSTRUCT_COMPATIBILITY_MODE
      output.WriteRawMessage(this);
    #else
      if (Code != 0) {
        output.WriteRawTag(8);
        output.WriteInt32(Code);
      }
      if (Message.Length != 0) {
        output.WriteRawTag(18);
        output.WriteString(Message);
      }
      metadata_.WriteTo(output, _map_metadata_codec);
      if (_unknownFields != null) {
        _unknownFields.WriteTo(output);
      }
    #endif
    }

    #if !GOOGLE_PROTOBUF_REFSTRUCT_COMPATIBILITY_MODE
    [global::System.Diagnostics.DebuggerNonUserCodeAttribute]
    [global::System.CodeDom.Compiler.GeneratedCode("protoc", null)]
    void pb::IBufferMessage.InternalWriteTo(ref pb::WriteContext output) {
      if (Code != 0) {
        output.WriteRawTag(8);
        output.WriteInt32(Code);
      }
      if (Message.Length != 0) {
        output.WriteRawTag(18);
        output.WriteString(Message);
      }
      metadata_.WriteTo(ref output, _map_metadata_codec);
      if (_unknownFields != null) {
        _unknownFields.WriteTo(ref output);
      }
    }
    #endif

    [global::System.Diagnostics.DebuggerNonUserCodeAttribute]
    [global::System.CodeDom.Compiler.GeneratedCode("protoc", null)]
    public int CalculateSize() {
      int size = 0;
      if (Code != 0) {
        size += 1 + pb::CodedOutputStream.ComputeInt32Size(Code);
      }
      if (Message.Length != 0) {
        size += 1 + pb::CodedOutputStream.ComputeStringSize(Message);
      }
      size += metadata_.CalculateSize(_map_metadata_codec);
      if (_unknownFields != null) {
        size += _unknownFields.CalculateSize();
      }
      return size;
    }

    [global::System.Diagnostics.DebuggerNonUserCodeAttribute]
    [global::System.CodeDom.Compiler.GeneratedCode("protoc", null)]
    public void MergeFrom(ErrorResponse other) {
      if (other == null) {
        return;
      }
      if (other.Code != 0) {
        Code = other.Code;
      }
      if (other.Message.Length != 0) {
        Message = other.Message;
      }
      metadata_.MergeFrom(other.metadata_);
      _unknownFields = pb::UnknownFieldSet.MergeFrom(_unknownFields, other._unknownFields);
    }

    [global::System.Diagnostics.DebuggerNonUserCodeAttribute]
    [global::System.CodeDom.Compiler.GeneratedCode("protoc", null)]
    public void MergeFrom(pb::CodedInputStream input) {
    #if !GOOGLE_PROTOBUF_REFSTRUCT_COMPATIBILITY_MODE
      input.ReadRawMessage(this);
    #else
      uint tag;
      while ((tag = input.ReadTag()) != 0) {
        switch(tag) {
          default:
            _unknownFields = pb::UnknownFieldSet.MergeFieldFrom(_unknownFields, input);
            break;
          case 8: {
            Code = input.ReadInt32();
            break;
          }
          case 18: {
            Message = input.ReadString();
            break;
          }
          case 26: {
            metadata_.AddEntriesFrom(input, _map_metadata_codec);
            break;
          }
        }
      }
    #endif
    }

    #if !GOOGLE_PROTOBUF_REFSTRUCT_COMPATIBILITY_MODE
    [global::System.Diagnostics.DebuggerNonUserCodeAttribute]
    [global::System.CodeDom.Compiler.GeneratedCode("protoc", null)]
    void pb::IBufferMessage.InternalMergeFrom(ref pb::ParseContext input) {
      uint tag;
      while ((tag = input.ReadTag()) != 0) {
        switch(tag) {
          default:
            _unknownFields = pb::UnknownFieldSet.MergeFieldFrom(_unknownFields, ref input);
            break;
          case 8: {
            Code = input.ReadInt32();
            break;
          }
          case 18: {
            Message = input.ReadString();
            break;
          }
          case 26: {
            metadata_.AddEntriesFrom(ref input, _map_metadata_codec);
            break;
          }
        }
      }
    }
    #endif

  }

  #endregion

}

#endregion Designer generated code
