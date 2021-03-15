using System.Collections.Generic;
using System.Threading.Tasks;
using AutoMapper;
using DTOs;
using Entities;
using Extensions;
using Helpers;
using Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Controllers
{
    [Authorize]
    public class MessagesController : BaseApiController
    {
        private readonly IUserRepository userReposiroty;
        private readonly IMessageRepository messageRepository;
        private readonly IMapper mapper;
        public MessagesController(IUserRepository userReposiroty, IMessageRepository messageRepository, IMapper mapper)
        {
            this.mapper = mapper;
            this.messageRepository = messageRepository;
            this.userReposiroty = userReposiroty;
        }

        [HttpPost]
        public async Task<ActionResult<MessageDto>> CreateMessage(CreateMessageDto createMessageDto)
        {
            var username = User.GetUsername();

            if (username == createMessageDto.RecipientUsername.ToLower()) return BadRequest("You Cannot Send Messages To Yourself");

            // Get hold of both our users in the sender and the recipient
            // As we need to populate the messages when we create it
            // And going the other way, we need to return a dto from this as well
            var sender = await this.userReposiroty.GetUserByUsernameAsync(username);
            var recipient = await this.userReposiroty.GetUserByUsernameAsync(createMessageDto.RecipientUsername);

            if (recipient == null) return NotFound();

            var message = new Message
            {
                Sender = sender,
                Recipient = recipient,
                SenderUsername = sender.UserName,
                RecipientUsername = recipient.UserName,
                Content = createMessageDto.Content
            };
            this.messageRepository.AddMessage(message);

            if (await this.messageRepository.SaveAllAsync()) return Ok(this.mapper.Map<MessageDto>(message));

            return BadRequest("Failed To Save Message");
        }

        [HttpGet]
        public async Task<ActionResult<IEnumerable<MessageDto>>> GetMessagesForUser([FromQuery] MessageParams messageParams)
        {
            messageParams.Username = User.GetUsername();

            var messages = await this.messageRepository.GetMessageForUser(messageParams);
            Response.AddPaginationHeader(messages.CurrentPage, messages.PageSize, messages.TotalCount, messages.TotalPages);

            return messages;
        }

        [HttpGet("thread/{username}")] // username here is other participant not current user
        public async Task<ActionResult<IEnumerable<MessageDto>>> GetMessageThread(string username)
        {
            var currentUsername = User.GetUsername();

            return Ok(await this.messageRepository.GetMessageThread(currentUsername, username));
        }

        [HttpDelete("{id}")] // Id of the message
        public async Task<ActionResult> DeleteMessage(int id)
        {
            var username = User.GetUsername();
            var message = await this.messageRepository.GetMessage(id);

            if (message.Sender.UserName != username && message.Recipient.UserName != username)
            {
                return Unauthorized();
            }
            if (message.Sender.UserName == username) message.SenderDeleted = true;
            if (message.Recipient.UserName == username) message.RecipientDeleted = true;

            if (message.SenderDeleted && message.RecipientDeleted)
            {
                this.messageRepository.DeleteMessage(message);
            }
            if (await this.messageRepository.SaveAllAsync()) return Ok();

            return BadRequest("Problem Deleting the Mesage!");
        }
    }
}