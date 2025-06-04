export interface Message {
  id: number
  senderId: number
  recipientId: number
  senderUsername: string
  senderPhotoUrl: string
  recipientPhotoUrl: string
  recipientUsername: string
  content: string
  dateRead?: Date
  messageSent: Date
}
